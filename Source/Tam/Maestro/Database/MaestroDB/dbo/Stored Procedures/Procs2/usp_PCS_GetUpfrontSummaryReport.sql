
CREATE PROCEDURE [dbo].[usp_PCS_GetUpfrontSummaryReport]
	-- Add the parameters for the stored procedure here
	@advertiser_id  int,
	@start_year		int, 
	@end_year		int,
	@start_quarter	int,
	@end_quarter	int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @numberOfYears int;
	SET @numberOfYears = @end_year - @start_year;
	DECLARE @currentQuarter int;
	DECLARE @currentYear int;

	CREATE TABLE #years_quarters
	(
		year int,
		quarter int,
	);
	
	IF @numberOfYears = 0
	BEGIN
		SET @currentQuarter = @start_quarter;
		WHILE (@currentQuarter <= @end_quarter)
		BEGIN
			INSERT INTO #years_quarters VALUES (@start_year, @currentQuarter);
			SET @currentQuarter = @currentQuarter + 1;			
		END
	END
	ELSE
	BEGIN
		SET @currentQuarter = @start_quarter;
		WHILE (@currentQuarter <= 4)
		BEGIN
			INSERT INTO #years_quarters VALUES (@start_year, @currentQuarter);
			SET @currentQuarter = @currentQuarter + 1;			
		END
		
		SET @currentYear = @start_year + 1;
		WHILE (@currentYear < @end_year)
		BEGIN
			SET @currentQuarter = 1;
			WHILE (@currentQuarter <= 4)
			BEGIN
				INSERT INTO #years_quarters VALUES (@currentYear, @currentQuarter);
				SET @currentQuarter = @currentQuarter + 1;
			END
			SET @currentYear = @currentYear + 1;
		END
		
		SET @currentQuarter = 1;
		WHILE (@currentQuarter <= @end_quarter)
		BEGIN
			INSERT INTO #years_quarters VALUES (@end_year, @currentQuarter);
			SET @currentQuarter = @currentQuarter + 1;			
		END
	END
	
	CREATE TABLE #proposals
	(
	    id           INT  ,
	    total_cost   MONEY,
	    quarter      INT  ,
	    year         INT  ,
	    is_overnight BIT  
	);
	
	INSERT INTO #proposals
	SELECT p.id,
		p.total_gross_cost,
		CASE mm.month 
			WHEN 1 THEN 1 
			WHEN 2 THEN 1 
			WHEN 3 THEN 1 
			WHEN 4 THEN 2 
			WHEN 5 THEN 2 
			WHEN 6 THEN 2 
			WHEN 7 THEN 3 
			WHEN 8 THEN 3 
			WHEN 9 THEN 3 
			WHEN 10 THEN 4 
			WHEN 11 THEN 4 
			WHEN 12 THEN 4 
	    END AS 'quarter',
	    mm.year,
	    dbo.IsOvernightPlan(p.id)
	FROM proposals AS p
	    	INNER JOIN
	        media_months AS mm
	        ON p.start_date BETWEEN mm.start_date AND mm.end_date
	        	AND mm.year = 2011
	WHERE  proposal_status_id = 4 OR proposal_status_id = 10
	    	AND is_upfront = 1
	    	AND p.advertiser_company_id = @advertiser_id
	
	SELECT  p.id,
	        SUM(CAST (pd.num_spots AS FLOAT) * (pdahh.us_universe * pd.universal_scaling_factor * pdahh.rating)) AS 'delivery_hh',
	        SUM(CAST (pd.num_spots AS FLOAT) * (pdademo.us_universe * pd.universal_scaling_factor * pdademo.rating)) AS 'delivery_demo',
	        total_cost,
	        a.name,
	        dbo.GetProposalTotalCPM(p.id, a.id) AS 'cpm_demo',
	        dbo.GetProposalTotalCPM(p.id, 31) AS 'cpm_hh',
	        p.quarter,
	        p.year,
	        p.is_overnight
	FROM    #proposals AS p
	        INNER JOIN
	        proposal_audiences AS pa
	        ON pa.proposal_id = p.id
	           AND pa.ordinal = 1
	        INNER JOIN
	        proposal_details AS pd
	        ON pd.proposal_id = p.id
	        INNER JOIN
	        proposal_detail_audiences AS pdahh
	        ON pdahh.proposal_detail_id = pd.id
	           AND pdahh.audience_id = 31
	        INNER JOIN
	        proposal_detail_audiences AS pdademo
	        ON pdademo.proposal_detail_id = pd.id
	           AND pdademo.audience_id = pa.audience_id
	        INNER JOIN
	        audiences AS a
	        ON a.id = pa.audience_id
	        INNER JOIN #years_quarters AS yq
	        ON p.year = yq.year
	        	AND p.quarter = yq.quarter
	GROUP BY p.id, p.total_cost, a.name, a.id, p.quarter, p.year, p.is_overnight;
	
	DROP TABLE #proposals;
	DROP TABLE #years_quarters;
END
