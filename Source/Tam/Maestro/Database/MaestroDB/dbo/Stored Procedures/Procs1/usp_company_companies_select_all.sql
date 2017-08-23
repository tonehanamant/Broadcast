CREATE PROCEDURE usp_company_companies_select_all
AS
SELECT
	*
FROM
	company_companies WITH(NOLOCK)
