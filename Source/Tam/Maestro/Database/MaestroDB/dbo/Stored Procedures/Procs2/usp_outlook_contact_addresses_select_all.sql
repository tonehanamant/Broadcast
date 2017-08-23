CREATE PROCEDURE usp_outlook_contact_addresses_select_all
AS
SELECT
	*
FROM
	outlook_contact_addresses WITH(NOLOCK)
