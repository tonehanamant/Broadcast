CREATE PROCEDURE usp_company_company_types_select_all
AS
SELECT
	*
FROM
	company_company_types WITH(NOLOCK)
