CREATE PROCEDURE usp_cmw_traffic_agencies_select_all
AS
SELECT
	*
FROM
	cmw_traffic_agencies WITH(NOLOCK)
