CREATE PROCEDURE usp_outlook_addresses_select_all
AS
SELECT
	*
FROM
	outlook_addresses WITH(NOLOCK)
