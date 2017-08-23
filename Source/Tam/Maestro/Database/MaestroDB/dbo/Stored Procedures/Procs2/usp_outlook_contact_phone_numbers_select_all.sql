CREATE PROCEDURE usp_outlook_contact_phone_numbers_select_all
AS
SELECT
	*
FROM
	outlook_contact_phone_numbers WITH(NOLOCK)
