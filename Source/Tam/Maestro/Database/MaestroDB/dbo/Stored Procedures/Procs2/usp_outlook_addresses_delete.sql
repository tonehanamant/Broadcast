CREATE PROCEDURE usp_outlook_addresses_delete
(
	@id Int
)
AS
DELETE FROM outlook_addresses WHERE id=@id
