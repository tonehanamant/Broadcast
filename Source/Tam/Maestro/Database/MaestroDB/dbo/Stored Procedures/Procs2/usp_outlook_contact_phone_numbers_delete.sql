CREATE PROCEDURE usp_outlook_contact_phone_numbers_delete
(
	@outlook_contact_id		Int,
	@outlook_phone_number_id		Int)
AS
DELETE FROM
	outlook_contact_phone_numbers
WHERE
	outlook_contact_id = @outlook_contact_id
 AND
	outlook_phone_number_id = @outlook_phone_number_id
