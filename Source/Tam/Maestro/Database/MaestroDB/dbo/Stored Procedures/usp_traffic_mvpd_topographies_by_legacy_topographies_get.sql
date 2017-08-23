
CREATE PROCEDURE usp_traffic_mvpd_topographies_by_legacy_topographies_get
(
@media_month_id INT
,@topography_id INT
)
AS
BEGIN
      SELECT DISTINCT z.id AS id 
      INTO #zoneTable
      FROM topographies t (NOLOCK) 
      LEFT OUTER JOIN frozen_topography_systems ts (NOLOCK) on ts.topography_id = t.id 
            AND ts.include = 1 
            AND ts.media_month_id = @media_month_id
      LEFT OUTER JOIN frozen_topography_zones tz (NOLOCK) on tz.topography_id = t.id 
            AND tz.include = 1
            AND tz.media_month_id = @media_month_id
      LEFT OUTER JOIN frozen_topography_system_groups tsg (NOLOCK) on tsg.topography_id = t.id 
            AND tsg.include = 1
            AND tsg.media_month_id = @media_month_id
      LEFT OUTER JOIN frozen_system_groups sg (NOLOCK) on sg.id = tsg.system_group_id
            AND sg.media_month_id = @media_month_id
      LEFT OUTER JOIN frozen_system_group_systems sgs (NOLOCK) on sgs.system_group_id = tsg.system_group_id 
            AND sgs.media_month_id = @media_month_id
      LEFT OUTER JOIN frozen_system_zones sz (NOLOCK) on (sz.system_id = sgs.system_id OR sz.system_id = ts.system_id)
            AND sz.media_month_id = @media_month_id
      LEFT OUTER JOIN frozen_systems (NOLOCK) s on (s.id = ts.system_id OR s.id = sz.system_id)
            AND s.media_month_id = @media_month_id
      LEFT OUTER JOIN frozen_zones z (NOLOCK) on (z.id = tz.zone_id OR sz.zone_id = z.id) 
            AND z.traffic = 1 
            AND z.media_month_id = @media_month_id
      WHERE t.id = @topography_id

      SELECT DISTINCT ft.* 
      FROM frozen_zone_businesses fzb (NOLOCK) 
      INNER JOIN frozen_topography_businesses ftb (NOLOCK) ON fzb.business_id = ftb.business_id 
            AND ftb.media_month_id = @media_month_id 
            AND ftb.include = 1
      INNER JOIN frozen_topographies ft (NOLOCK) on ft.id = ftb.topography_id
            AND ft.media_month_id = @media_month_id 
      WHERE ft.topography_type = 2
            AND fzb.media_month_id = @media_month_id
            AND fzb.type = 'MANAGEDBY'
            AND fzb.zone_id IN (
                  SELECT id 
                  FROM #zoneTable)
                  
      DROP TABLE #zoneTable;
END