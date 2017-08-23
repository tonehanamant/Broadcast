CREATE PROCEDURE usp_contact_phone_numbers_insert
(
	@contact_id		Int,
	@phone_number_id		Int
)
AS
INSERT INTO contact_phone_numbers
(
	contact_id,
	phone_number_id
)
VALUES
(
	@contact_id,
	@phone_number_id
)

