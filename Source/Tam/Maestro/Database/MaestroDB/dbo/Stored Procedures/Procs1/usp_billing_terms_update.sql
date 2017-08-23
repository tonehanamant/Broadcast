CREATE PROCEDURE [dbo].[usp_billing_terms_update]
(
	@id		Int,
	@code		VarChar(31),
	@name		VarChar(63),
	@order_by		Int,
	@is_default		Bit
)
AS
UPDATE dbo.billing_terms SET
	code = @code,
	name = @name,
	order_by = @order_by,
	is_default = @is_default
WHERE
	id = @id
