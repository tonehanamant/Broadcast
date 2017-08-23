CREATE PROCEDURE usp_cmw_invoice_adjustments_delete
(
	@id Int
)
AS
DELETE FROM cmw_invoice_adjustments WHERE id=@id
