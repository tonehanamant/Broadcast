CREATE PROCEDURE usp_phone_numbers_select_all
AS
SELECT
	*
FROM
	phone_numbers WITH(NOLOCK)
