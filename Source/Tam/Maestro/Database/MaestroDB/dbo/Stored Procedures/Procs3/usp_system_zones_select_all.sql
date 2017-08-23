CREATE PROCEDURE usp_system_zones_select_all
AS
SELECT
	*
FROM
	system_zones WITH(NOLOCK)
