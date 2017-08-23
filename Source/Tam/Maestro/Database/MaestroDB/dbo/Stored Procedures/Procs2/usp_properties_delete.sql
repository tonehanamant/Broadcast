CREATE PROCEDURE usp_properties_delete
(
	@id Int
)
AS
DELETE FROM properties WHERE id=@id
