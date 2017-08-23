
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
-- usp_FOG_DetermineAffordableProposalInfo2 104,385,408594
CREATE PROCEDURE [dbo].[usp_FOG_DetermineAffordableProposalInfo2]
	@topography_id INT, 
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @total_impressions FLOAT
	DECLARE @quarter INT, @year INT
	DECLARE @start_date DATETIME
	DECLARE @end_date DATETIME
	
	SELECT
		@quarter = CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END,
		@year = mm.year,
		@start_date=mm.start_date,
		@end_date=mm.end_date
	FROM
		media_months mm (NOLOCK)
	WHERE
		mm.id=@media_month_id
			
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
				AND p.is_audience_deficiency_unit_schedule=0
				AND p.is_upfront=0
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
		dp.*
	FROM
		uvw_display_proposals dp
	WHERE
		dp.id IN (
			SELECT DISTINCT proposal_id FROM #affordable_proposals
		)	
				
	IF OBJECT_ID('tempdb..#affordable_proposals') IS NOT NULL DROP TABLE #affordable_proposals;
	IF OBJECT_ID('tempdb..#dayparts') IS NOT NULL DROP TABLE #dayparts;
	IF OBJECT_ID('tempdb..#proposal_details') IS NOT NULL DROP TABLE #proposal_details;
	IF OBJECT_ID('tempdb..#max_cpms') IS NOT NULL DROP TABLE #max_cpms;
END
