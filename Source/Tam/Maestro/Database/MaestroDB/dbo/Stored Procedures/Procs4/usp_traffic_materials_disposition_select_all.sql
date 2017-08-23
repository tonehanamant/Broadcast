CREATE PROCEDURE usp_traffic_materials_disposition_select_all
AS
SELECT
	*
FROM
	traffic_materials_disposition WITH(NOLOCK)
