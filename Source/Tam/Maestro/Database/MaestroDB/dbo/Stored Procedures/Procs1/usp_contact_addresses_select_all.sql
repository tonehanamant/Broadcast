CREATE PROCEDURE usp_contact_addresses_select_all
AS
SELECT
	*
FROM
	contact_addresses WITH(NOLOCK)
