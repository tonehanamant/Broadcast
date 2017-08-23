CREATE PROCEDURE usp_phone_numbers_delete
(
	@id Int
)
AS
DELETE FROM phone_numbers WHERE id=@id
