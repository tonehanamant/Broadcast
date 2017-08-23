CREATE PROCEDURE usp_outlook_contacts_select_all
AS
SELECT
	*
FROM
	outlook_contacts WITH(NOLOCK)
