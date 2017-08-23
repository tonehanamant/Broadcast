CREATE PROCEDURE usp_nielsen_network_histories_select_all
AS
SELECT
	*
FROM
	nielsen_network_histories WITH(NOLOCK)
