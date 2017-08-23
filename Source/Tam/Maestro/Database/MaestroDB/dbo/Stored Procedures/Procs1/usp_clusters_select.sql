CREATE PROCEDURE usp_clusters_select
(
	@id Int
)
AS
SELECT
	*
FROM
	clusters WITH(NOLOCK)
WHERE
	id = @id
