CREATE PROCEDURE usp_contact_phone_numbers_delete
(
	@contact_id		Int,
	@phone_number_id		Int)
AS
DELETE FROM
	contact_phone_numbers
WHERE
	contact_id = @contact_id
 AND
	phone_number_id = @phone_number_id
