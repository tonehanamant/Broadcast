CREATE PROCEDURE usp_cmw_contacts_select_all
AS
SELECT
	*
FROM
	cmw_contacts WITH(NOLOCK)
