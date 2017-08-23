CREATE PROCEDURE usp_cmw_traffic_materials_select_all
AS
SELECT
	*
FROM
	cmw_traffic_materials WITH(NOLOCK)
