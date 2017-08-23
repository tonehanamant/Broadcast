CREATE PROCEDURE usp_outlook_phone_numbers_update
(
	@id		Int,
	@phone_number_type_id		Int,
	@phone_number		VarChar(15),
	@extension		VarChar(15)
)
AS
UPDATE outlook_phone_numbers SET
	phone_number_type_id = @phone_number_type_id,
	phone_number = @phone_number,
	extension = @extension
WHERE
	id = @id

