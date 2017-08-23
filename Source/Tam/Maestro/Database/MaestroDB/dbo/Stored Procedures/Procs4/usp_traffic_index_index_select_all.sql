CREATE PROCEDURE usp_traffic_index_index_select_all
AS
SELECT
	*
FROM
	traffic_index_index WITH(NOLOCK)
