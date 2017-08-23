CREATE PROCEDURE usp_addresses_delete
(
	@id Int
)
AS
DELETE FROM addresses WHERE id=@id
