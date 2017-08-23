CREATE PROCEDURE usp_email_profiles_select_all
AS
SELECT
	*
FROM
	email_profiles WITH(NOLOCK)
