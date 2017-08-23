CREATE PROCEDURE usp_substitution_categories_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(15),
	@description		VarChar(63)
)
AS
INSERT INTO substitution_categories
(
	name,
	description
)
VALUES
(
	@name,
	@description
)

SELECT
	@id = SCOPE_IDENTITY()

