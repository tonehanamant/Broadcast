CREATE PROCEDURE usp_company_types_select_all
AS
SELECT
	*
FROM
	company_types WITH(NOLOCK)
