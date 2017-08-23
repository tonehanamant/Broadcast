CREATE PROCEDURE usp_addresses_select
(
	@id Int
)
AS
SELECT
	*
FROM
	addresses WITH(NOLOCK)
WHERE
	id = @id
