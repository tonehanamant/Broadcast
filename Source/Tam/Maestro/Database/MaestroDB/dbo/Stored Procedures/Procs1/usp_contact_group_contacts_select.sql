CREATE PROCEDURE usp_contact_group_contacts_select
(
	@contact_id		Int,
	@contact_group_id		Int
)
AS
SELECT
	*
FROM
	contact_group_contacts WITH(NOLOCK)
WHERE
	contact_id=@contact_id
	AND
	contact_group_id=@contact_group_id

