CREATE PROCEDURE usp_outlook_contacts_select
(
	@id Int
)
AS
SELECT
	*
FROM
	outlook_contacts WITH(NOLOCK)
WHERE
	id = @id
