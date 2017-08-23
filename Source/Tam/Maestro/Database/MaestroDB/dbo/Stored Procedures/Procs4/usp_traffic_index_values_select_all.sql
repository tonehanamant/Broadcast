CREATE PROCEDURE usp_traffic_index_values_select_all
AS
SELECT
	*
FROM
	traffic_index_values WITH(NOLOCK)
