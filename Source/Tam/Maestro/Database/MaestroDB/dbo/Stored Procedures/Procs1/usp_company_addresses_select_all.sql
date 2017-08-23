CREATE PROCEDURE usp_company_addresses_select_all
AS
SELECT
	*
FROM
	company_addresses WITH(NOLOCK)
