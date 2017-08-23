CREATE PROCEDURE usp_system_statement_contact_infos_select_all
AS
SELECT
	*
FROM
	system_statement_contact_infos WITH(NOLOCK)
