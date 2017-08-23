CREATE PROCEDURE usp_cmw_traffic_advertisers_select_all
AS
SELECT
	*
FROM
	cmw_traffic_advertisers WITH(NOLOCK)
