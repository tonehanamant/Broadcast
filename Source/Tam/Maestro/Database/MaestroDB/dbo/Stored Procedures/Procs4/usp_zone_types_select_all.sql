CREATE PROCEDURE usp_zone_types_select_all
AS
BEGIN
SELECT
	*
FROM
	zone_types WITH(NOLOCK)
END
