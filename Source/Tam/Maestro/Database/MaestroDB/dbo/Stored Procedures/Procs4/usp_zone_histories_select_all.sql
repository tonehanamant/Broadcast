CREATE PROCEDURE usp_zone_histories_select_all
AS
BEGIN
SELECT
	*
FROM
	zone_histories WITH(NOLOCK)
END
