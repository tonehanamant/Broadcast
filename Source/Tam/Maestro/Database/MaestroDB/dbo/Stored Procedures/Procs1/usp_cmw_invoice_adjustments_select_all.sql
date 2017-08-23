CREATE PROCEDURE usp_cmw_invoice_adjustments_select_all
AS
SELECT
	*
FROM
	cmw_invoice_adjustments WITH(NOLOCK)
