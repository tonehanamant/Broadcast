-- usp_MCS_GetReelBusinessObjectBySystemGroup 242,3
CREATE PROCEDURE [dbo].[usp_MCS_GetReelBusinessObjectBySystemGroup]
	@reel_id INT,
	@system_group_id INT
AS
BEGIN
	DECLARE @effective_date as datetime;
	SET @effective_date = getdate();
	
	DECLARE @lreel_id INT;
	SET @lreel_id = @reel_id;
	
	DECLARE @lsystem_group_id INT;
	SET @lsystem_group_id = @system_group_id;
	
	CREATE TABLE #systems (system_id int);
	
	INSERT INTO #systems
	SELECT 
		system_id 
	FROM 
		uvw_system_universe (NOLOCK) 
	WHERE 
		system_id IN (
			SELECT system_id FROM uvw_systemgroupsystem_universe (NOLOCK) WHERE 
				system_group_id=@lsystem_group_id
				AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
		) 
		AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
		AND (active=1);
		
		SELECT
					distinct ra.*
  		FROM
					reel_materials rm WITH (NOLOCK)
					JOIN reel_advertisers ra WITH (NOLOCK) on ra.reel_id = rm.reel_id and ra.line_number = rm.line_number
					JOIN traffic_materials tm WITH (NOLOCK) on tm.reel_material_id = rm.id 
					JOIN traffic_details td WITH (NOLOCK) on td.traffic_id = tm.traffic_id
					JOIN traffic_orders tro WITH (NOLOCK) on tro.traffic_detail_id = td.id
					JOIN #systems WITH (NOLOCK) on #systems.system_id = tro.system_id
	WHERE
					rm.reel_id=@lreel_id 
					and tro.ordered_spots > 0 and tro.active = 1 and tro.on_financial_reports = 1
	
	
	SELECT
					distinct rm.id,
					rm.reel_id, 
					rm.material_id, 
					rm.cut,
					rm.line_number,
					rm.active,
					uvw_display_materials.*
	FROM
					reel_materials rm WITH (NOLOCK)
					join uvw_display_materials WITH (NOLOCK) on uvw_display_materials.material_id = rm.material_id
					JOIN traffic_materials tm WITH (NOLOCK) on tm.reel_material_id = rm.id 
					JOIN traffic_details td WITH (NOLOCK) on td.traffic_id = tm.traffic_id
					JOIN traffic_orders tro WITH (NOLOCK) on tro.traffic_detail_id = td.id
					JOIN #systems WITH (NOLOCK) on #systems.system_id = tro.system_id
	WHERE
					rm.reel_id=@lreel_id 
					and tro.ordered_spots > 0 and tro.active = 1 and tro.on_financial_reports = 1
	ORDER BY
					rm.line_number, rm.cut

	DROP TABLE #systems;

END
