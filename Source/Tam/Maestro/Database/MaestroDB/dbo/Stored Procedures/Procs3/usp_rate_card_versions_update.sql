CREATE PROCEDURE [dbo].[usp_rate_card_versions_update]
(
	@id		Int,
	@name		VarChar(15),
	@is_default		Bit
)
AS
UPDATE rate_card_versions SET
	name = @name,
	is_default = @is_default
WHERE
	id = @id

