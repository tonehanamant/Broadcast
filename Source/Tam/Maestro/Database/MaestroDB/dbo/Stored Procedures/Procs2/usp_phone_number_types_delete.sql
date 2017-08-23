CREATE PROCEDURE usp_phone_number_types_delete
(
	@id Int
)
AS
DELETE FROM phone_number_types WHERE id=@id
