CREATE PROCEDURE usp_system_zone_histories_select_all
AS
SELECT
	*
FROM
	system_zone_histories WITH(NOLOCK)
