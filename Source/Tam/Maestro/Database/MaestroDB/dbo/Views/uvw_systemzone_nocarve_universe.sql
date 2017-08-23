CREATE VIEW [dbo].[uvw_systemzone_nocarve_universe]
AS
SELECT     zone_id, system_id, type, effective_date AS start_date, NULL AS end_date
FROM         dbo.system_zones (NOLOCK) WHERE type<>'CARVE'
UNION ALL
SELECT     zone_id, system_id, type, start_date, end_date
FROM         dbo.system_zone_histories (NOLOCK) WHERE type<>'CARVE'
