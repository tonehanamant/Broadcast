CREATE PROCEDURE usp_phone_numbers_insert
(
	@id		Int		OUTPUT,
	@phone_number_type_id		Int,
	@phone_number		VarChar(15),
	@extension		VarChar(15),
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO phone_numbers
(
	phone_number_type_id,
	phone_number,
	extension,
	date_created,
	date_last_modified
)
VALUES
(
	@phone_number_type_id,
	@phone_number,
	@extension,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

