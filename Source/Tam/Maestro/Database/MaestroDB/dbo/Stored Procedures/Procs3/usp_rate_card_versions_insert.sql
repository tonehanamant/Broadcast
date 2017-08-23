CREATE PROCEDURE [dbo].[usp_rate_card_versions_insert]
(
	@id		int		OUTPUT,
	@name		VarChar(15),
	@is_default		Bit
)
AS
INSERT INTO rate_card_versions
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

