CREATE PROCEDURE usp_network_breaks_select
(
	@id Int
)
AS
SELECT
	*
FROM
	network_breaks WITH(NOLOCK)
WHERE
	id = @id
