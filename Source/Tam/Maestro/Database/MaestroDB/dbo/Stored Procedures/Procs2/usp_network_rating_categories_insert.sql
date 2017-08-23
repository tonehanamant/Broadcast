CREATE PROCEDURE usp_network_rating_categories_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(15),
	@description		VarChar(63)
)
AS
INSERT INTO network_rating_categories
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

