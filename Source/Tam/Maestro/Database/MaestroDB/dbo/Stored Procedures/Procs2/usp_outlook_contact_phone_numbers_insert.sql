CREATE PROCEDURE usp_outlook_contact_phone_numbers_insert
(
	@outlook_contact_id		Int,
	@outlook_phone_number_id		Int
)
AS
INSERT INTO outlook_contact_phone_numbers
(
	outlook_contact_id,
	outlook_phone_number_id
)
VALUES
(
	@outlook_contact_id,
	@outlook_phone_number_id
)

