CREATE PROCEDURE usp_cmw_invoice_details_select_all
AS
SELECT
	*
FROM
	cmw_invoice_details WITH(NOLOCK)
