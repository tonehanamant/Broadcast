CREATE PROCEDURE usp_rating_categories_insert
(
	@id		Int,
	@code		VarChar(15),
	@name		VarChar(63)
)
AS
INSERT INTO rating_categories
(
	id,
	code,
	name
)
VALUES
(
	@id,
	@code,
	@name
)

