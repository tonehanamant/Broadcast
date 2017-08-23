CREATE PROCEDURE usp_network_substitution_histories_select_all
AS
SELECT
	*
FROM
	network_substitution_histories WITH(NOLOCK)
