CREATE PROCEDURE usp_cmw_bill_adjustments_insert
(
	@cmw_bill_id		Int,
	@cmw_invoice_adjustment_id		Int
)
AS
INSERT INTO cmw_bill_adjustments
(
	cmw_bill_id,
	cmw_invoice_adjustment_id
)
VALUES
(
	@cmw_bill_id,
	@cmw_invoice_adjustment_id
)

