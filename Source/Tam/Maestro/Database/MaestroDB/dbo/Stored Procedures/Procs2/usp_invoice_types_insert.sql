
CREATE PROCEDURE [dbo].[usp_invoice_types_insert]
(
	@id		Int		OUTPUT,
	@Description		VarChar(50)
)
AS
INSERT INTO invoice_types
(
	Description
)
VALUES
(
	@Description
)

SELECT
	@id = SCOPE_IDENTITY()


