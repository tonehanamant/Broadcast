-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/16/2014
-- Description:	<Description,,>
-- =============================================
-- usp_PLS_CreateAndGetTemporaryTraffickedComponentDaypartBreakoutsTable 389
CREATE PROCEDURE [dbo].[usp_PLS_CreateAndGetTemporaryTraffickedComponentDaypartBreakoutsTable]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @start_date DATETIME;
	DECLARE @end_date DATETIME;
	
	SELECT
		@start_date = mm.start_date,
		@end_date = mm.end_date
	FROM
		media_months mm (NOLOCK)
	WHERE
		mm.id=@media_month_id;

    IF OBJECT_ID('tempdb..#component_dayparts') IS NOT NULL DROP TABLE #component_dayparts
	CREATE TABLE #component_dayparts (daypart_id INT, start_time INT, end_time INT, mon BIT, tue BIT, wed BIT, thu BIT, fri BIT, sat BIT, sun BIT, total_hours FLOAT)
		INSERT INTO #component_dayparts
			SELECT
				d.id,
				d.start_time,
				d.end_time,
				d.mon,
				d.tue,
				d.wed,
				d.thu,
				d.fri,
				d.sat,
				d.sun,
				d.total_hours
			FROM
				dbo.vw_ccc_daypart d
				JOIN dbo.daypart_maps dm (NOLOCK) ON dm.map_set='HistAvails' 
					AND dm.daypart_id=d.id;

	IF OBJECT_ID('tempdb..#trafficked_dayparts') IS NOT NULL DROP TABLE #trafficked_dayparts
	CREATE TABLE #trafficked_dayparts (daypart_id INT)
		INSERT INTO #trafficked_dayparts
			SELECT DISTINCT
				tro.daypart_id
			FROM
				traffic_orders tro (NOLOCK)
			WHERE
				tro.start_date between @start_date and @end_date
				AND tro.release_id IS NOT NULL
				AND tro.on_financial_reports=1
				AND tro.active=1;
	      
	CREATE TABLE ##trafficked_component_breakout_percentages (traffic_daypart_id INT, component_daypart_id INT, percentage FLOAT, intersecting_component_traffic_daypart_percentage FLOAT)
		INSERT INTO ##trafficked_component_breakout_percentages
			SELECT 
				td.daypart_id 'traffic_daypart_id',
				cd.daypart_id 'component_daypart_id',
				CAST(dbo.GetIntersectingDaypartDays(dt.mon,dt.tue,dt.wed,dt.thu,dt.fri,dt.sat,dt.sun, cd.mon,cd.tue,cd.wed,cd.thu,cd.fri,cd.sat,cd.sun) * dbo.GetIntersectingDaypartHours(dt.start_time,dt.end_time,cd.start_time,cd.end_time) AS FLOAT) / dt.total_hours 'percentage',
				(dbo.GetIntersectingDaypartDays(dt.mon,dt.tue,dt.wed,dt.thu,dt.fri,dt.sat,dt.sun, cd.mon,cd.tue,cd.wed,cd.thu,cd.fri,cd.sat,cd.sun) * dbo.GetIntersectingDaypartHours(dt.start_time,dt.end_time,cd.start_time,cd.end_time)) / cd.total_hours 'intersecting_component_traffic_daypart_percentage'
			FROM 
				#trafficked_dayparts td
				JOIN vw_ccc_daypart dt ON dt.id=td.daypart_id
				JOIN #component_dayparts cd ON dbo.GetIntersectingDaypartDays(dt.mon,dt.tue,dt.wed,dt.thu,dt.fri,dt.sat,dt.sun, cd.mon,cd.tue,cd.wed,cd.thu,cd.fri,cd.sat,cd.sun)>0
					AND dbo.GetIntersectingDaypartHours(dt.start_time,dt.end_time,cd.start_time,cd.end_time)>0;
	
	SELECT
		d.id,d.code,d.name,d.start_time,d.end_time,d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun
	FROM
		##trafficked_component_breakout_percentages tcbp
		JOIN vw_ccc_daypart d ON d.id=tcbp.component_daypart_id	
	UNION ALL
	SELECT
		d.id,d.code,d.name,d.start_time,d.end_time,d.mon,d.tue,d.wed,d.thu,d.fri,d.sat,d.sun
	FROM
		#trafficked_dayparts td
		JOIN vw_ccc_daypart d ON d.id=td.daypart_id;
		
	DROP TABLE #component_dayparts;
	DROP TABLE #trafficked_dayparts;
END
