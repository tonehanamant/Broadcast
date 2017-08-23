CREATE PROCEDURE usp_outlook_companies_select_all
AS
SELECT
	*
FROM
	outlook_companies WITH(NOLOCK)
