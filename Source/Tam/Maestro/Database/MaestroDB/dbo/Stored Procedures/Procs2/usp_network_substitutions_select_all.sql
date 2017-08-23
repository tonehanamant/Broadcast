CREATE PROCEDURE usp_network_substitutions_select_all
AS
SELECT
	*
FROM
	network_substitutions WITH(NOLOCK)
