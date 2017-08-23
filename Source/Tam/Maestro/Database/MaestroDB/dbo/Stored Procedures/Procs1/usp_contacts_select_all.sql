CREATE PROCEDURE usp_contacts_select_all
AS
SELECT
	*
FROM
	contacts WITH(NOLOCK)
