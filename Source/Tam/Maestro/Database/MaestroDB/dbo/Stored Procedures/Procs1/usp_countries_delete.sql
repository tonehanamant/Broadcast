CREATE PROCEDURE usp_countries_delete
(
	@id Int
)
AS
DELETE FROM countries WHERE id=@id
