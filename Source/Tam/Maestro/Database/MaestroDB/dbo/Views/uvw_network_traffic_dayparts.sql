
CREATE VIEW [dbo].[uvw_network_traffic_dayparts]
AS
	SELECT
	     nielsen_network_id, daypart_id, effective_date AS start_date, NULL AS end_date
	FROM
		dbo.network_traffic_dayparts(NOLOCK)
		
	UNION ALL
	
	SELECT
		nielsen_network_id, daypart_id, [start_date] , end_date
	FROM
		dbo.network_traffic_daypart_histories(NOLOCK)