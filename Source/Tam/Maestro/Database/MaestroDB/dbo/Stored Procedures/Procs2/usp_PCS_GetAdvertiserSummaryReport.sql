/* =============================================
-- Author:		
-- Create date: <Create Date,,>
-- Description:	<Description,,>

-- mod id: 1.1
-- modification: 11/15/2012  
-- Eric Wenger
-- Description: per requirements from the business, 
	--exclude upfront plans; and
	--exclude Hispanic networks
	--change proc to look at ordered plan worksheets,
	no longer at posting plans/posts.  This is to resolve
	the issue with a delta between Advertiser Summary Report 
	and Revenue by Network Report totals.
	
		other changes:
	--SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED in place of table hints
	--put reserved words used as column names in brackets
	--explicitly specify the DB object schema
	--check if temp table exists and drop if does
	
	--posting plan id column changed to ordered plan
	--verify against Revenue by Network Report
	--since looking at worksheets, need to account for possible null values
	
--to execute:
--	EXEC [dbo].[usp_PCS_GetAdvertiserSummaryReport] 2011, 2011, 3, 3
-- ============================================= */

CREATE PROCEDURE [dbo].[usp_PCS_GetAdvertiserSummaryReport]
	@start_year		int, 
	@end_year		int,
	@start_quarter	int,
	@end_quarter	int
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED -- +1.1 sql2K8 may disregard table level hints

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

/* --declarations for independent query
DECLARE
	@start_year		int, 
	@end_year		int,
	@start_quarter	int,
	@end_quarter	int;

	SET @start_year=2011; 
	@end_year=2011; 
	@start_quarter=4
	@end_quarter=4;

*/

	DECLARE @numberOfYears int;
	SET @numberOfYears = @end_year - @start_year;
	DECLARE @currentQuarter int;
	DECLARE @currentYear int;

IF OBJECT_ID('tempdb..#years_quarters') IS NOT NULL DROP TABLE #years_quarters; -- +1.1 check if temp table exists and drop if does
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

	-- +1.1 revised query
	SELECT 
		isnull(udp.agency,'<agency not specified>'), -- +1.1 account for null
		isnull(udp.advertiser,'<advertiser not specified>'), -- +1.1 account for null
		isnull(udp.product,'<product not specified>'), -- +1.1 account for null
		p.id ordered_plan_proposal_id, --posting_plan_proposal_id, -- +1.1 change to show correct type of plan
		SUM(
			CAST(pdw.units AS MONEY)
			*
			pd.proposal_rate
		) 'total_cost',
				CASE mm.[month] 
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
		mm.[year]
	FROM
		dbo.proposal_detail_worksheets pdw -- +1.1 specify object schema
		JOIN dbo.proposal_details pd ON pd.id=pdw.proposal_detail_id  -- +1.1 specify object schema
		JOIN dbo.proposals p ON p.id=pd.proposal_id  -- +1.1 specify object schema
		JOIN dbo.networks n ON n.id=pd.network_id and n.code not like '%-H%' -- +1.1 exclude Hispanic networks; specify object schema
		JOIN dbo.media_weeks mw ON mw.id=pdw.media_week_id -- +1.1 specify object schema
		JOIN dbo.media_months mm ON mm.id=mw.media_month_id -- +1.1 specify object schema
		JOIN dbo.uvw_display_proposals udp		ON udp.id = p.id	  -- +1.1 add join to get more descriptive plan fields
		INNER JOIN #years_quarters AS yq 			 ON yq.[year] = mm.[year] AND yq.quarter = CASE mm.[month]  -- +1.1 bracket reserved words
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
	    END
	WHERE
		p.proposal_status_id=4 --ordered plan
		AND pdw.units>0
		AND pd.proposal_rate>0
		AND p.is_upfront =0 /* +1.1 exclude upfronts; query already returns ordered plans, which is the most reliable source for this flag value */
	GROUP BY
		p.id,
		udp.agency,
		udp.advertiser,
		udp.product,
		mm.[month], -- +1.1 bracket reserved words
		mm.[year] -- +1.1 bracket reserved words


/*  -- -1.1 replaced query
	SELECT DISTINCT	
		db.agency,
		db.advertiser,
		db.product,
		tpp.posting_plan_proposal_id,
		SUM(
			CAST(pd.num_spots AS MONEY)
			*
			pd.proposal_rate
		) 'total_cost',
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
	FROM
		tam_posts tp							(NOLOCK)
		INNER JOIN tam_post_proposals tpp		(NOLOCK) ON tpp.tam_post_id=tp.id
		INNER JOIN proposal_details pd			(NOLOCK) ON pd.proposal_id=tpp.posting_plan_proposal_id
		INNER JOIN spot_lengths sl				(NOLOCK) ON sl.id=pd.spot_length_id
		INNER JOIN proposals p					(NOLOCK) ON p.id=tpp.posting_plan_proposal_id
		INNER JOIN media_months mm				(NOLOCK) ON mm.id=p.posting_media_month_id
		INNER JOIN uvw_display_proposals db				 ON db.id = tpp.posting_plan_proposal_id
		INNER JOIN #years_quarters AS yq 				 ON yq.year = mm.year AND yq.quarter = CASE mm.month 
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
	    END
	WHERE
		tp.is_deleted=0			-- posts that haven't been market deleted
		AND tp.post_type_code=1 -- posts that have been marked "Official"
		AND tpp.is_fast_track=0 -- is not a fast track
	GROUP BY
		tpp.posting_plan_proposal_id,
		db.agency,
		db.advertiser,
		db.product,
		mm.month,
		mm.year
*/
	IF OBJECT_ID('tempdb..#years_quarters') IS NOT NULL DROP TABLE #years_quarters; -- +1.1 check if temp table exists and drop if does
END
