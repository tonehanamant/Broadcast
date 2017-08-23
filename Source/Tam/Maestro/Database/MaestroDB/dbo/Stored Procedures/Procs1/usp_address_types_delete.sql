CREATE PROCEDURE usp_address_types_delete
(
	@id Int
)
AS
DELETE FROM address_types WHERE id=@id
