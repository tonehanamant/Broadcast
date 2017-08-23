
CREATE PROCEDURE [dbo].[usp_invoice_types_update]
(
	@id		Int,
	@Description		VarChar(50)
)
AS
UPDATE invoice_types SET
	Description = @Description
WHERE
	id = @id


