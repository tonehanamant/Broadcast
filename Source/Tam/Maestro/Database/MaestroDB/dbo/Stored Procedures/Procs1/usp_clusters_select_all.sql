CREATE PROCEDURE usp_clusters_select_all
AS
SELECT
	*
FROM
	clusters WITH(NOLOCK)
