CREATE PROCEDURE usp_billing_categories_update
(
	@id		Int,
	@name		VarChar(63),
	@is_default		Bit
)
AS
UPDATE billing_categories SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id

