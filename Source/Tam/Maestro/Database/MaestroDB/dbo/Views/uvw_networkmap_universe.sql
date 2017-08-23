CREATE VIEW [dbo].[uvw_networkmap_universe]
AS
SELECT     id AS network_map_id, network_id, map_set, map_value, active, flag, effective_date AS start_date, NULL AS end_date
FROM         dbo.network_maps (NOLOCK)
UNION ALL
SELECT     network_map_id, network_id, map_set, map_value, active, flag, start_date, end_date
FROM         dbo.network_map_histories (NOLOCK)