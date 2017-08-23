CREATE PROCEDURE usp_phone_number_types_select_all
AS
SELECT
	*
FROM
	phone_number_types WITH(NOLOCK)
