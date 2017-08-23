CREATE PROCEDURE usp_outlook_phone_numbers_delete
(
	@id Int
)
AS
DELETE FROM outlook_phone_numbers WHERE id=@id
