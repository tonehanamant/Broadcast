CREATE PROCEDURE usp_contact_methods_select
(
	@id Int
)
AS
SELECT
	*
FROM
	contact_methods WITH(NOLOCK)
WHERE
	id = @id
