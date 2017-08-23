CREATE PROCEDURE usp_traffic_index_values_select
(
	@id Int
)
AS
SELECT
	*
FROM
	traffic_index_values WITH(NOLOCK)
WHERE
	id = @id
