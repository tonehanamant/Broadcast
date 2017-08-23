CREATE PROCEDURE usp_cmw_traffic_select_all
AS
SELECT
	*
FROM
	cmw_traffic WITH(NOLOCK)
