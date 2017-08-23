CREATE PROCEDURE usp_cmw_contacts_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_contacts WITH(NOLOCK)
WHERE
	id = @id
