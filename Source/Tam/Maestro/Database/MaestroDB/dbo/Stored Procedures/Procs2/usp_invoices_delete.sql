CREATE PROCEDURE [dbo].[usp_invoices_delete]
(
	@id Int
)
AS
BEGIN
DELETE FROM dbo.invoices WHERE id=@id
END
