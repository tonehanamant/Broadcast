-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/22/2014 03:01:18 PM
-- Description:	Auto-generated method to insert a billing_terms record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_billing_terms_insert]
	@id INT OUTPUT,
	@code VARCHAR(31),
	@name VARCHAR(63),
	@order_by INT,
	@is_default BIT
AS
BEGIN
	INSERT INTO [dbo].[billing_terms]
	(
		[code],
		[name],
		[order_by],
		[is_default]
	)
	VALUES
	(
		@code,
		@name,
		@order_by,
		@is_default
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
