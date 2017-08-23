CREATE PROCEDURE usp_network_break_histories_select_all
AS
SELECT
	*
FROM
	network_break_histories WITH(NOLOCK)
