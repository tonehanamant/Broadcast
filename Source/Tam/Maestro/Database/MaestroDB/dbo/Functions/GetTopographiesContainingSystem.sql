
-- =============================================
-- Author:		Joshua Jewell
-- Create date: 10/6/2015
-- Description:	Given the system_id and effective_date returns a list of topography id's contain this system.
-- =============================================
CREATE FUNCTION [dbo].[GetTopographiesContainingSystem]
(	
	@system_id INT,
	@effective_date DATETIME
)
RETURNS @return TABLE
(
	topography_id INT
) 
AS
BEGIN
	INSERT INTO @return

	SELECT DISTINCT topography_id FROM
	(
		-- TOPOGRAPHY SYSTEMS
		SELECT 
			tsu.topography_id 
		FROM 
			uvw_topography_system_universe tsu (NOLOCK) 
		WHERE 
			tsu.start_date<=@effective_date AND (tsu.end_date>=@effective_date OR tsu.end_date IS NULL) 
			AND tsu.system_id=@system_id
			AND tsu.include=1

		UNION ALL

		-- TOPOGRAPHY SYSTEM GROUPS
		SELECT 
			tsgu.topography_id 
		FROM 
			uvw_topography_system_group_universe tsgu (NOLOCK)
			JOIN uvw_systemgroupsystem_universe sgsu (NOLOCK) ON sgsu.system_group_id=tsgu.system_group_id AND (sgsu.start_date<=@effective_date AND (sgsu.end_date>=@effective_date OR sgsu.end_date IS NULL))
		WHERE
			tsgu.start_date<=@effective_date AND (tsgu.end_date>=@effective_date OR tsgu.end_date IS NULL)
			AND tsgu.include=1
			AND sgsu.system_id=@system_id
		
		UNION ALL

		-- TOPOGRAPHY SYSTEMS
		select tb.topography_id
			from uvw_system_universe s WITH (NOLOCK)
			join uvw_systemzone_universe sz WITH (NOLOCK)
				on sz.system_id = s.system_id
				and sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL) 
			join uvw_zone_universe z WITH (NOLOCK)
				on z.zone_id = sz.zone_id
				and z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL) 
			join uvw_zonebusiness_universe zb WITH (NOLOCK)
				on zb.zone_id = z.zone_id
				and zb.start_date<=@effective_date AND (zb.end_date>=@effective_date OR zb.end_date IS NULL) 
			join uvw_business_universe b WITH (NOLOCK)
				on b.business_id = zb.business_id
				and b.start_date<=@effective_date AND (b.end_date>=@effective_date OR b.end_date IS NULL) 
			join uvw_topography_business_universe tb WITH (NOLOCK)
				on tb.business_id = b.business_id
				and tb.start_date<=@effective_date AND (tb.end_date>=@effective_date OR tb.end_date IS NULL) 
			join topographies t WITH (NOLOCK)
				on t.id = tb.topography_id
			where s.system_id = @system_id
				and s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL) 
				and s.active = 1
				and t.topography_type = 2

			UNION ALL

		-- TOPOGRAPHY SYSTEM GROUPS
		select tb.topography_id
			from uvw_systemgroupsystem_universe s WITH (NOLOCK)
			join uvw_systemgroup_universe su WITH (NOLOCK)
				on su.system_group_id = s.system_group_id
				and su.start_date<=@effective_date AND (su.end_date>=@effective_date OR su.end_date IS NULL) 
			join uvw_systemzone_universe sz WITH (NOLOCK)
				on sz.system_id = s.system_id
				and sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL) 
			join uvw_zone_universe z WITH (NOLOCK)
				on z.zone_id = sz.zone_id
				and z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL) 
			join uvw_zonebusiness_universe zb WITH (NOLOCK)
				on zb.zone_id = z.zone_id
				and zb.start_date<=@effective_date AND (zb.end_date>=@effective_date OR zb.end_date IS NULL) 
			join uvw_business_universe b WITH (NOLOCK)
				on b.business_id = zb.business_id
				and b.start_date<=@effective_date AND (b.end_date>=@effective_date OR b.end_date IS NULL) 
			join uvw_topography_business_universe tb WITH (NOLOCK)
				on tb.business_id = b.business_id
				and tb.start_date<=@effective_date AND (tb.end_date>=@effective_date OR tb.end_date IS NULL) 
			join topographies t WITH (NOLOCK)
				on t.id = tb.topography_id
			where s.system_id = @system_id
				and s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL) 
				and su.active = 1
				and t.topography_type = 2
	) tmp

	RETURN;
END



