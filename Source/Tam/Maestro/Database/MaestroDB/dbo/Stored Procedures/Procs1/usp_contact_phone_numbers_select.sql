CREATE PROCEDURE usp_contact_phone_numbers_select
(
	@contact_id		Int,
	@phone_number_id		Int
)
AS
SELECT
	*
FROM
	contact_phone_numbers WITH(NOLOCK)
WHERE
	contact_id=@contact_id
	AND
	phone_number_id=@phone_number_id

