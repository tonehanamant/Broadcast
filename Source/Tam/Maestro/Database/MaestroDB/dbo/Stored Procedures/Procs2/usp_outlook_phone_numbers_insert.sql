CREATE PROCEDURE usp_outlook_phone_numbers_insert
(
	@id		Int		OUTPUT,
	@phone_number_type_id		Int,
	@phone_number		VarChar(15),
	@extension		VarChar(15)
)
AS
INSERT INTO outlook_phone_numbers
(
	phone_number_type_id,
	phone_number,
	extension
)
VALUES
(
	@phone_number_type_id,
	@phone_number,
	@extension
)

SELECT
	@id = SCOPE_IDENTITY()

