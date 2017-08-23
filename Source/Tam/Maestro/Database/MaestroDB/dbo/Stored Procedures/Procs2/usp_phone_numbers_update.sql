CREATE PROCEDURE usp_phone_numbers_update
(
	@id		Int,
	@phone_number_type_id		Int,
	@phone_number		VarChar(15),
	@extension		VarChar(15),
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE phone_numbers SET
	phone_number_type_id = @phone_number_type_id,
	phone_number = @phone_number,
	extension = @extension,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

