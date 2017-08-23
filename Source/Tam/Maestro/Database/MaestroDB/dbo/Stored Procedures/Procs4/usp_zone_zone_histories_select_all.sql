CREATE PROCEDURE usp_zone_zone_histories_select_all
AS
SELECT
	*
FROM
	zone_zone_histories WITH(NOLOCK)
