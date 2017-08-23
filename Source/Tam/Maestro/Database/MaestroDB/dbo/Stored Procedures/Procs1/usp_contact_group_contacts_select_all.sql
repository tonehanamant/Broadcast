CREATE PROCEDURE usp_contact_group_contacts_select_all
AS
SELECT
	*
FROM
	contact_group_contacts WITH(NOLOCK)
