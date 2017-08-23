CREATE PROCEDURE usp_cmw_contact_infos_select_all
AS
SELECT
	*
FROM
	cmw_contact_infos WITH(NOLOCK)
