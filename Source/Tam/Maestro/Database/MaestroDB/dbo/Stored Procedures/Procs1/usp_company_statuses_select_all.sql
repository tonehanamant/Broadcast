CREATE PROCEDURE usp_company_statuses_select_all
AS
SELECT
	*
FROM
	company_statuses WITH(NOLOCK)
