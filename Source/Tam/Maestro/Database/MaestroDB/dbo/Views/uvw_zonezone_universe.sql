CREATE VIEW [dbo].[uvw_zonezone_universe]
AS
SELECT     primary_zone_id, secondary_zone_id, type, effective_date AS start_date, NULL AS end_date
FROM         dbo.zone_zones (NOLOCK)
UNION ALL
SELECT     primary_zone_id, secondary_zone_id, type, start_date, end_date
FROM         dbo.zone_zone_histories (NOLOCK)
