CREATE PROCEDURE usp_categories_insert
(
	@id		Int		OUTPUT,
	@category_set		VarChar(15),
	@name		VarChar(15),
	@description		VarChar(63)
)
AS
INSERT INTO categories
(
	category_set,
	name,
	description
)
VALUES
(
	@category_set,
	@name,
	@description
)

SELECT
	@id = SCOPE_IDENTITY()

