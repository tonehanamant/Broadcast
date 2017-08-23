CREATE PROCEDURE usp_cmw_invoices_select_all
AS
SELECT
	*
FROM
	cmw_invoices WITH(NOLOCK)
