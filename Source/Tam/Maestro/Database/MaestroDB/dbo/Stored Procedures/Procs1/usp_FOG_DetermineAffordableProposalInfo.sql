-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
-- usp_FOG_DetermineAffordableProposalInfo 104,385,408594
CREATE PROCEDURE [dbo].[usp_FOG_DetermineAffordableProposalInfo]
	@topography_id INT, 
	@media_month_id INT,
	@total_quarterly_dollars MONEY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @total_impressions FLOAT
	DECLARE @total_weekly_dollars MONEY
	DECLARE @quarter INT, @year INT
	DECLARE @start_date DATETIME
	DECLARE @end_date DATETIME
	DECLARE @hiatus_weeks FlightTable;
	
	SELECT
		@quarter = CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END,
		@year = mm.year,
		@start_date=mm.start_date,
		@end_date=mm.end_date
	FROM
		media_months mm (NOLOCK)
	WHERE
		mm.id=@media_month_id
		
	SET @total_weekly_dollars = (@total_quarterly_dollars / (SELECT COUNT(1) FROM media_weeks mw (NOLOCK) JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id AND mm.year=@year AND @quarter=CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END)) * (SELECT COUNT(1) FROM media_weeks mw (NOLOCK) WHERE mw.media_month_id=@media_month_id)
	
 	IF OBJECT_ID('tempdb..#dayparts') IS NOT NULL DROP TABLE #dayparts;
    CREATE TABLE #dayparts (daypart_id INT, start_time INT, end_time INT, mon BIT, tue BIT, wed BIT, thu BIT, fri BIT, sat BIT, sun BIT)
	INSERT INTO #dayparts
		SELECT DISTINCT 
			d.id,
			d.start_time,
			d.end_time,
			d.mon,
			d.tue,
			d.wed,
			d.thu,
			d.fri,
			d.sat,
			d.sun 
		FROM 
			static_inventories si (NOLOCK) 
			JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id AND mw.media_month_id=@media_month_id 
			JOIN vw_ccc_daypart d ON d.id=si.daypart_id 
		WHERE 
			si.topography_id=@topography_id 
			AND si.[enable]=1

	IF OBJECT_ID('tempdb..#networks_by_week') IS NOT NULL DROP TABLE #networks_by_week;
	CREATE TABLE #networks_by_week (network_id INT, media_week_id INT)
	INSERT INTO #networks_by_week
		SELECT DISTINCT
			si.network_id,
			si.media_week_id
		FROM
			static_inventories si (NOLOCK)
			JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
				AND mw.media_month_id=@media_month_id
		WHERE
			si.topography_id=@topography_id
			AND si.[enable]=1
		
	IF OBJECT_ID('tempdb..#subs') IS NOT NULL DROP TABLE #subs;
	CREATE TABLE #subs (media_week_id INT, network_id INT, daypart_id INT, subscribers INT)
	INSERT INTO #subs
		SELECT
			si.media_week_id,
			si.network_id,
			si.daypart_id,
			SUM(CASE WHEN zn.subscribers IS NULL THEN 0 ELSE zn.subscribers * si.available_units END) 'subscribers'
		FROM
			static_inventories si (NOLOCK)
			JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
				AND mw.media_month_id=@media_month_id
			JOIN media_months mm (NOLOCK) ON mm.id=@media_month_id
			LEFT JOIN uvw_zonenetwork_universe zn ON zn.zone_id=si.zone_id AND zn.network_id=si.network_id AND zn.start_date<=mm.start_date AND (zn.end_date>=mm.start_date OR zn.end_date IS NULL)
		WHERE
			si.topography_id=@topography_id
			AND si.[enable]=1
		GROUP BY
			si.media_week_id,
			si.network_id,
			si.daypart_id
			
	IF OBJECT_ID('tempdb..#ratings') IS NOT NULL DROP TABLE #ratings;
	CREATE TABLE #ratings (network_id INT, daypart_id INT, rating FLOAT)
	INSERT INTO #ratings
		SELECT
			nbw.network_id,
			d.daypart_id,
			r.rating
		FROM
			#networks_by_week nbw
			JOIN media_weeks mw (NOLOCK) ON mw.id=nbw.media_week_id
			CROSS APPLY #dayparts d (NOLOCK)
			CROSS APPLY dbo.udf_GetCustomRatings(nbw.network_id,31,@media_month_id-3,mw.start_date,mw.end_date,d.start_time,d.end_time,d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun,2,1,@hiatus_weeks,NULL) r
		ORDER BY
			nbw.network_id,
			d.daypart_id
		
	-- sum total impressions
	SELECT
		@total_impressions = SUM(s.subscribers * r.rating)
	FROM
		#subs s
		JOIN #ratings r ON r.network_id=s.network_id AND r.daypart_id=s.daypart_id
		
	IF OBJECT_ID('tempdb..#impressions_by_net_by_week') IS NOT NULL DROP TABLE #impressions_by_net_by_week;
	CREATE TABLE #impressions_by_net_by_week (media_week_id INT, network_id INT, impressions FLOAT, network_week_dollars MONEY)
	INSERT INTO #impressions_by_net_by_week
		SELECT
			s.media_week_id,
			s.network_id,
			SUM(s.subscribers * ISNULL(r.rating,0)) 'impressions',
			(SUM(s.subscribers * ISNULL(r.rating,0)) / @total_impressions) * @total_weekly_dollars 'network_week_dollars'
		FROM
			#subs s
			LEFT JOIN #ratings r ON r.network_id=s.network_id AND r.daypart_id=s.daypart_id
		GROUP BY
			s.media_week_id,
			s.network_id
			
	IF OBJECT_ID('tempdb..#impressions_by_net_by_week_by_daypart') IS NOT NULL DROP TABLE #impressions_by_net_by_week_by_daypart;
	CREATE TABLE #impressions_by_net_by_week_by_daypart (media_week_id INT, network_id INT, daypart_id INT, impressions FLOAT)
	INSERT INTO #impressions_by_net_by_week_by_daypart
		SELECT
			s.media_week_id,
			s.network_id,
			s.daypart_id,
			SUM(s.subscribers * ISNULL(r.rating, 0)) 'impressions'
		FROM
			#subs s
			LEFT JOIN #ratings r ON r.network_id=s.network_id AND r.daypart_id=s.daypart_id
		GROUP BY
			s.media_week_id,
			s.network_id,
			s.daypart_id
	
	-- return dollars_per_daypart for each week/network/daypart, this is the number which will be broken down by zone
	SELECT
		d.media_week_id,
		d.network_id,
		d.daypart_id,
		CASE w.impressions WHEN 0 THEN 0 ELSE CAST((d.impressions / w.impressions) * w.network_week_dollars AS MONEY) END 'dollars_per_daypart' -- rate to traffic
	FROM 
		#impressions_by_net_by_week_by_daypart d
		JOIN #impressions_by_net_by_week w ON w.network_id=d.network_id AND w.media_week_id=d.media_week_id	
		
	-- use the plan with the highest cpm per network, week, and daypart.
	IF OBJECT_ID('tempdb..#proposal_details') IS NOT NULL DROP TABLE #proposal_details;
	CREATE TABLE #proposal_details (proposal_id INT, network_id INT, media_week_id INT, daypart_id INT, hh_cpm MONEY)
	INSERT INTO #proposal_details
		SELECT 
			dp.id,
			pd.network_id,
			mw.id 'media_week_id',
			dc.daypart_id,
			CASE 
				WHEN ((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) > 0.0 THEN
					CAST(pd.proposal_rate / ((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) AS MONEY)
				ELSE
					0.0
			END 'hh_cpm'
		FROM 
			uvw_display_proposals dp
			JOIN proposals p (NOLOCK) ON p.id=dp.id
				AND p.multiple_dayparts=0
				AND p.multiple_spot_lengths=0
			JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=dp.id
				AND pd.spot_length_id=1 -- :30 plans
				AND pd.num_spots > 0 -- must have units on that network
			JOIN proposal_detail_audiences pda (NOLOCK) ON pda.proposal_detail_id=pd.id AND pda.audience_id=31
			JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
				AND (d.mon + d.tue + d.wed + d.thu + d.fri + d.sat + d.sun) >= 5 -- daypart must be at least 5 days
				AND NOT ((d.start_time BETWEEN 72000 AND 86400 OR d.start_time BETWEEN 0 AND 21599) AND d.end_time BETWEEN 0 AND 21599) -- exclude overnight
			JOIN #dayparts dc ON dc.start_time BETWEEN d.start_time AND d.end_time -- intersect component dayparts (full)
			JOIN proposal_flights pf (NOLOCK) ON pf.proposal_id=dp.id
				AND pf.selected=1 -- non-hiatus weeks
			JOIN media_weeks mw (NOLOCK) ON (mw.start_date <= pf.end_date AND mw.end_date >= pf.start_date)
				AND mw.media_month_id=@media_month_id
		WHERE 
			dp.proposal_status_id=4 -- ordered plans
			AND dp.advertiser NOT LIKE '%Chattem%'
			AND dp.advertiser NOT LIKE '%Various%'
			AND dp.product NOT LIKE '%Various%'
		ORDER BY
			dp.id,
			pd.network_id,
			dc.daypart_id

	IF OBJECT_ID('tempdb..#max_cpms') IS NOT NULL DROP TABLE #max_cpms;
	CREATE TABLE #max_cpms (network_id INT, media_week_id INT, daypart_id INT, hh_cpm MONEY)
	INSERT INTO #max_cpms
		SELECT
			network_id,
			media_week_id,
			daypart_id,
			MAX(hh_cpm) -- best cpm in the mix by net/week/daypart
		FROM
			#proposal_details pd
		GROUP BY
			network_id,
			media_week_id,
			daypart_id


	IF OBJECT_ID('tempdb..#affordable_proposals') IS NOT NULL DROP TABLE #affordable_proposals;
	CREATE TABLE #affordable_proposals (media_week_id INT, network_id INT, daypart_id INT, proposal_id INT)
	INSERT INTO #affordable_proposals
		SELECT
			pd.media_week_id,
			pd.network_id,
			pd.daypart_id,
			MIN(pd.proposal_id) 'proposal_id'
		FROM
			#proposal_details pd
			JOIN #max_cpms mc ON mc.network_id=pd.network_id 
				AND mc.media_week_id=pd.media_week_id 
				AND mc.daypart_id=pd.daypart_id 
				AND mc.hh_cpm=pd.hh_cpm
		GROUP BY
			pd.network_id,
			pd.media_week_id,
			pd.daypart_id
		
	SELECT
		ap.*
	FROM
		#affordable_proposals ap	
	
	SELECT
		*
	FROM
		#subs
		
	SELECT
		dp.*
	FROM
		uvw_display_proposals dp
	WHERE
		dp.id IN (
			SELECT DISTINCT proposal_id FROM #affordable_proposals
		)	
				
	IF OBJECT_ID('tempdb..#affordable_proposals') IS NOT NULL DROP TABLE #affordable_proposals;
	IF OBJECT_ID('tempdb..#dayparts') IS NOT NULL DROP TABLE #dayparts;
	IF OBJECT_ID('tempdb..#networks_by_week') IS NOT NULL DROP TABLE #networks_by_week;
	IF OBJECT_ID('tempdb..#subs') IS NOT NULL DROP TABLE #subs;
	IF OBJECT_ID('tempdb..#ratings') IS NOT NULL DROP TABLE #ratings;
	IF OBJECT_ID('tempdb..#impressions_by_net_by_week') IS NOT NULL DROP TABLE #impressions_by_net_by_week;
	IF OBJECT_ID('tempdb..#impressions_by_net_by_week_by_daypart') IS NOT NULL DROP TABLE #impressions_by_net_by_week_by_daypart;
	IF OBJECT_ID('tempdb..#proposal_details') IS NOT NULL DROP TABLE #proposal_details;
	IF OBJECT_ID('tempdb..#max_cpms') IS NOT NULL DROP TABLE #max_cpms;
END
