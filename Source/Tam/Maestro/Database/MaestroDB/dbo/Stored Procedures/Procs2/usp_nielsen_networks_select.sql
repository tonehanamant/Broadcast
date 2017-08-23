CREATE PROCEDURE usp_nielsen_networks_select
(
	@id Int
)
AS
SELECT
	*
FROM
	nielsen_networks WITH(NOLOCK)
WHERE
	id = @id
