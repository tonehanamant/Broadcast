-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
-- usp_FOG_SetInventoryRates 104,385,408594
CREATE PROCEDURE [dbo].[usp_FOG_SetMonthlyInventoryRates]
	@topography_id INT, 
	@media_month_id INT,
	@total_quarterly_dollars MONEY,
	@base_ratings_media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @total_impressions FLOAT
	DECLARE @total_monthly_dollars MONEY
	DECLARE @quarter INT, @year INT
	DECLARE @start_date DATETIME
	DECLARE @end_date DATETIME
	DECLARE @hiatus_weeks FlightTable;
	
	-- reset
	UPDATE 
		static_inventories
	SET
		rate = NULL
	FROM
		static_inventories si
		JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id 
			AND mw.media_month_id=@media_month_id
	WHERE 
		si.topography_id=@topography_id 
	
	SELECT
		@quarter = CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END,
		@year = mm.year,
		@start_date=mm.start_date,
		@end_date=mm.end_date
	FROM
		media_months mm (NOLOCK)
	WHERE
		mm.id=@media_month_id
		
	SET @total_monthly_dollars = (@total_quarterly_dollars / (SELECT COUNT(1) FROM media_weeks mw (NOLOCK) JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id AND mm.year=@year AND @quarter=CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END)) 
	SET @total_monthly_dollars = @total_monthly_dollars * (SELECT COUNT(1) FROM media_weeks mw (NOLOCK) WHERE mw.media_month_id=@media_month_id)
	
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
			LEFT JOIN uvw_zonenetwork_universe zn ON zn.zone_id=si.zone_id 
				AND zn.network_id=si.network_id 
				AND zn.start_date<=@start_date AND (zn.end_date>=@start_date OR zn.end_date IS NULL)
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
			CROSS APPLY dbo.udf_GetCustomRatings(nbw.network_id,31,@base_ratings_media_month_id,mw.start_date,mw.end_date,d.start_time,d.end_time,d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun,2,1,@hiatus_weeks,NULL) r
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
			(SUM(s.subscribers * ISNULL(r.rating,0)) / @total_impressions) * @total_monthly_dollars 'network_week_dollars'
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
	IF OBJECT_ID('tempdb..#dollars_by_daypart') IS NOT NULL DROP TABLE #dollars_by_daypart;
	CREATE TABLE #dollars_by_daypart (media_week_id INT, network_id INT, daypart_id INT, dollars_per_daypart FLOAT)
	INSERT INTO #dollars_by_daypart
		SELECT
			d.media_week_id,
			d.network_id,
			d.daypart_id,
			CASE w.impressions WHEN 0 THEN 0 ELSE CAST((d.impressions / w.impressions) * w.network_week_dollars AS MONEY) END 'dollars_per_daypart' -- rate to traffic
		FROM 
			#impressions_by_net_by_week_by_daypart d
			JOIN #impressions_by_net_by_week w ON w.network_id=d.network_id AND w.media_week_id=d.media_week_id
		
		
	UPDATE 
		static_inventories
	SET
		rate = (dbd.dollars_per_daypart * ((CAST(zn.subscribers AS FLOAT) * CAST(si.available_units AS FLOAT)) / CAST(s.subscribers AS FLOAT)) / CAST(si.available_units AS FLOAT))
	FROM
		static_inventories si
		JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id AND mw.media_month_id=@media_month_id
		JOIN #subs s ON s.media_week_id=si.media_week_id
			AND s.network_id=si.network_id
			AND s.daypart_id=si.daypart_id
		JOIN uvw_zonenetwork_universe zn ON zn.zone_id=si.zone_id 
			AND zn.network_id=si.network_id 
			AND zn.start_date<=@start_date AND (zn.end_date>=@start_date OR zn.end_date IS NULL)
		JOIN #dollars_by_daypart dbd ON dbd.media_week_id=si.media_week_id 
			AND dbd.network_id=si.network_id
			AND dbd.daypart_id=si.daypart_id
	WHERE 
		si.topography_id=@topography_id 
		AND si.[enable]=1
		AND si.available_units>0
		
				
	IF OBJECT_ID('tempdb..#dayparts') IS NOT NULL DROP TABLE #dayparts;
	IF OBJECT_ID('tempdb..#networks_by_week') IS NOT NULL DROP TABLE #networks_by_week;
	IF OBJECT_ID('tempdb..#subs') IS NOT NULL DROP TABLE #subs;
	IF OBJECT_ID('tempdb..#ratings') IS NOT NULL DROP TABLE #ratings;
	IF OBJECT_ID('tempdb..#impressions_by_net_by_week') IS NOT NULL DROP TABLE #impressions_by_net_by_week;
	IF OBJECT_ID('tempdb..#impressions_by_net_by_week_by_daypart') IS NOT NULL DROP TABLE #impressions_by_net_by_week_by_daypart;
END
