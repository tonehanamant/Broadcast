CREATE PROCEDURE [dbo].[usp_rate_card_types_insert]
(
	@id		int		OUTPUT,
	@name		VarChar(63),
	@is_default		Bit
)
AS
INSERT INTO rate_card_types
(
	name,
	is_default
)
VALUES
(
	@name,
	@is_default
)

SELECT
	@id = @@Identity
