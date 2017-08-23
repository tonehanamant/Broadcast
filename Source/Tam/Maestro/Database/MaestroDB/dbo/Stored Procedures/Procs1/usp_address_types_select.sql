CREATE PROCEDURE usp_address_types_select
(
	@id Int
)
AS
SELECT
	*
FROM
	address_types WITH(NOLOCK)
WHERE
	id = @id
