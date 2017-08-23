CREATE PROCEDURE usp_contact_phone_numbers_select_all
AS
SELECT
	*
FROM
	contact_phone_numbers WITH(NOLOCK)
