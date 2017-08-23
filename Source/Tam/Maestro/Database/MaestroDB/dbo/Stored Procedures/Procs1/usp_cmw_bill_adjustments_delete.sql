CREATE PROCEDURE usp_cmw_bill_adjustments_delete
(
	@cmw_bill_id		Int,
	@cmw_invoice_adjustment_id		Int)
AS
DELETE FROM
	cmw_bill_adjustments
WHERE
	cmw_bill_id = @cmw_bill_id
 AND
	cmw_invoice_adjustment_id = @cmw_invoice_adjustment_id
