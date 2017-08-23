

CREATE VIEW [dbo].[uvw_network_maps]
AS
	SELECT
	     id 'network_map_id', network_id, map_set, map_value, active, flag, effective_date AS start_date, NULL AS end_date
	FROM
		dbo.network_maps(NOLOCK)
		
	UNION ALL
	
	SELECT
		network_map_id, network_id, map_set, map_value, active, flag, start_date AS Expr1, end_date
	FROM
		dbo.network_map_histories(NOLOCK)


