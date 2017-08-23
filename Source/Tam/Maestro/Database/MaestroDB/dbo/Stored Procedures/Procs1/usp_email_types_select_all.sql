CREATE PROCEDURE usp_email_types_select_all
AS
SELECT
	*
FROM
	email_types WITH(NOLOCK)
