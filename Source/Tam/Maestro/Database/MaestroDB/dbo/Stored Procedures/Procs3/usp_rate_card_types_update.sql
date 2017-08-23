CREATE PROCEDURE [dbo].[usp_rate_card_types_update]
(
	@id		Int,
	@name		VarChar(63),
	@is_default		Bit
)
AS
UPDATE rate_card_types SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id
