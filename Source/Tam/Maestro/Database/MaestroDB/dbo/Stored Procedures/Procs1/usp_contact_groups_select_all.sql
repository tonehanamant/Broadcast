CREATE PROCEDURE usp_contact_groups_select_all
AS
SELECT
	*
FROM
	contact_groups WITH(NOLOCK)
