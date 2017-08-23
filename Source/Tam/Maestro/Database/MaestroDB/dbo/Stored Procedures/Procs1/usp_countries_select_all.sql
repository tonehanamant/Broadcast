CREATE PROCEDURE usp_countries_select_all
AS
SELECT
	*
FROM
	countries WITH(NOLOCK)
