CREATE PROCEDURE usp_cmw_contact_infos_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_contact_infos WITH(NOLOCK)
WHERE
	id = @id
