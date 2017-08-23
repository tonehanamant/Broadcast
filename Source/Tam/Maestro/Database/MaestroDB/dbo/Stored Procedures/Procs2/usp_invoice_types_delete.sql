
CREATE PROCEDURE [dbo].[usp_invoice_types_delete]
(
	@id Int
)
AS
DELETE FROM invoice_types WHERE id=@id

