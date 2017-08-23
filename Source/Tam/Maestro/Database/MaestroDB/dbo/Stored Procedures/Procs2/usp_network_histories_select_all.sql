CREATE PROCEDURE usp_network_histories_select_all
AS
BEGIN
SELECT
	*
FROM
	dbo.network_histories WITH(NOLOCK)
END
