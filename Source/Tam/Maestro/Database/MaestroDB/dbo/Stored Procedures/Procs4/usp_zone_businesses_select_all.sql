CREATE PROCEDURE usp_zone_businesses_select_all
AS
SELECT
	*
FROM
	zone_businesses WITH(NOLOCK)
