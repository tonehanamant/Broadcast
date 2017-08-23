CREATE PROCEDURE usp_cmw_traffic_details_select_all
AS
SELECT
	*
FROM
	cmw_traffic_details WITH(NOLOCK)
