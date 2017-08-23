/****** Object:  Table [dbo].[usp_REL_GetActiveTopographiesForTraffic]    Script Date: 11/16/2012 14:51:25 ******/
CREATE PROCEDURE [dbo].[usp_REL_GetActiveTopographiesForTraffic]
(
	@traffic_id int
)
AS
	declare @effective_date datetime;

	SELECT @effective_date = MIN(start_date) 
		from traffic_orders too WITH (NOLOCK) join traffic_details td WITH (NOLOCK) on too.traffic_detail_id = td.id
	WHERE
		td.traffic_id = @traffic_id and too.on_financial_reports = 1 and too.ordered_spots > 0;

	IF(@effective_date IS NULL)
                SELECT @effective_date = start_date from traffic WITH (NOLOCK) where traffic.id = @traffic_id;

	CREATE TABLE #DistinctSystems (system_id int);
	INSERT INTO #DistinctSystems (system_id)
		SELECT 
			DISTINCT system_id 
		from 
			traffic_orders too WITH (NOLOCK) 
			join traffic_details td WITH (NOLOCK) on too.traffic_detail_id = td.id
		WHERE
			td.traffic_id = @traffic_id and too.on_financial_reports = 1 and too.active = 1;

	CREATE TABLE #DistinctTopographies (topography_id int);
	INSERT INTO #DistinctTopographies(topography_id) 
		SELECT DISTINCT		
			tdt.topography_id 
		FROM
			traffic_details td WITH (NOLOCK) 
			JOIN traffic_detail_weeks tdw WITH (NOLOCK) on td.id = tdw.traffic_detail_id 
			JOIN traffic_detail_topographies tdt WITH (NOLOCK) on tdt.traffic_detail_week_id = tdw.id
		WHERE
			td.traffic_id = @traffic_id;

	SELECT DISTINCT tmp.topography_id, tp.code, tp.name, CAST(tm.map_value as int) FROM
	(
		-- TOPOGRAPHY SYSTEMS
		SELECT 
			tsu.topography_id 
		FROM 
			uvw_topography_system_universe tsu (NOLOCK) 
			JOIN #DistinctSystems DS on DS.system_id = tsu.system_id
		WHERE 
			tsu.start_date<=@effective_date AND (tsu.end_date>=@effective_date OR tsu.end_date IS NULL) 
			AND tsu.include=1

		UNION ALL

		-- TOPOGRAPHY SYSTEM GROUPS
		SELECT 
			tsgu.topography_id 
		FROM 
			uvw_topography_system_group_universe tsgu (NOLOCK)
			JOIN uvw_systemgroupsystem_universe sgsu (NOLOCK) ON sgsu.system_group_id=tsgu.system_group_id AND (sgsu.start_date<=@effective_date AND (sgsu.end_date>=@effective_date OR sgsu.end_date IS NULL))
			JOIN #DistinctSystems DS on DS.system_id = sgsu.system_id                     
		WHERE
			tsgu.start_date<=@effective_date AND (tsgu.end_date>=@effective_date OR tsgu.end_date IS NULL)
			AND tsgu.include=1
	) tmp
	join #DistinctTopographies with (NOLOCK) on #DistinctTopographies.topography_id = tmp.topography_id
	join topographies tp with (NOLOCK) on tp.id = tmp.topography_id
	LEFT join topography_maps tm WITH (NOLOCK) on tm.topography_id = tp.id AND tm.map_set = 'traffic'
	ORDER BY
		CAST(tm.map_value as int);

	DROP TABLE #DistinctSystems;
	DROP TABLE #DistinctTopographies;
