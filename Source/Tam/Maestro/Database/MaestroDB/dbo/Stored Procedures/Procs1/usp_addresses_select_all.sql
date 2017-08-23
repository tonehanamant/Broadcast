CREATE PROCEDURE usp_addresses_select_all
AS
SELECT
	*
FROM
	addresses WITH(NOLOCK)
