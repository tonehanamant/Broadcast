CREATE PROCEDURE usp_traffic_index_index_select
(
	@id Int
)
AS
SELECT
	*
FROM
	traffic_index_index WITH(NOLOCK)
WHERE
	id = @id
