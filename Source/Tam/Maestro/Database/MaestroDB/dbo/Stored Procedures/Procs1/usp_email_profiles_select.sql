CREATE PROCEDURE usp_email_profiles_select
(
	@id Int
)
AS
SELECT
	*
FROM
	email_profiles WITH(NOLOCK)
WHERE
	id = @id
