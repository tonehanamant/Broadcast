CREATE PROCEDURE usp_outlook_phone_numbers_select_all
AS
SELECT
	*
FROM
	outlook_phone_numbers WITH(NOLOCK)
