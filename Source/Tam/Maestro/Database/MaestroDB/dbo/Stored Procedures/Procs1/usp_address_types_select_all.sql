CREATE PROCEDURE usp_address_types_select_all
AS
SELECT
	*
FROM
	address_types WITH(NOLOCK)
