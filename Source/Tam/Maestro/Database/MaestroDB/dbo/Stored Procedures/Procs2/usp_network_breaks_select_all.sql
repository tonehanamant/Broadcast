CREATE PROCEDURE usp_network_breaks_select_all
AS
SELECT
	*
FROM
	network_breaks WITH(NOLOCK)
