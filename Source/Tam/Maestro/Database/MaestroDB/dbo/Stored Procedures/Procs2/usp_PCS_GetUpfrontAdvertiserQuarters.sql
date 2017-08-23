
CREATE PROCEDURE [dbo].[usp_PCS_GetUpfrontAdvertiserQuarters]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT DISTINCT 
		p.advertiser_company_id,
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
		mm.year
	FROM proposals AS p
		INNER JOIN media_months AS mm
			ON p.start_date BETWEEN mm.start_date AND mm.end_date
	WHERE  proposal_status_id = 4 OR proposal_status_id = 10
			AND is_upfront = 1
			AND p.advertiser_company_id is not null
	ORDER BY p.advertiser_company_id, year, quarter asc
END

