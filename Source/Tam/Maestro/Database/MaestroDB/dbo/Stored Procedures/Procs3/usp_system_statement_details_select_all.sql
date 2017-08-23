CREATE PROCEDURE usp_system_statement_details_select_all
AS
SELECT
	*
FROM
	system_statement_details WITH(NOLOCK)
