CREATE PROCEDURE usp_cmw_invoices_delete
(
	@id Int
)
AS
DELETE FROM cmw_invoices WHERE id=@id
