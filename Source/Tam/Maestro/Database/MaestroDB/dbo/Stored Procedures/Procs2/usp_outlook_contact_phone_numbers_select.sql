CREATE PROCEDURE usp_outlook_contact_phone_numbers_select
(
	@outlook_contact_id		Int,
	@outlook_phone_number_id		Int
)
AS
SELECT
	*
FROM
	outlook_contact_phone_numbers WITH(NOLOCK)
WHERE
	outlook_contact_id=@outlook_contact_id
	AND
	outlook_phone_number_id=@outlook_phone_number_id

