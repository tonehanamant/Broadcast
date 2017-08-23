CREATE PROCEDURE usp_nielsen_networks_select_all
AS
SELECT
	*
FROM
	nielsen_networks WITH(NOLOCK)
