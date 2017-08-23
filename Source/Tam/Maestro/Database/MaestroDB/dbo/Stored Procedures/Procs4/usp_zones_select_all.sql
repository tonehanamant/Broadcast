CREATE PROCEDURE usp_zones_select_all
AS
BEGIN
SELECT
	*
FROM
	zones WITH(NOLOCK)
END
