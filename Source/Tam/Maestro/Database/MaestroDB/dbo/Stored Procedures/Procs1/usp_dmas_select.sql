CREATE PROCEDURE usp_dmas_select
(
	@id Int
)
AS
SELECT
	*
FROM
	dmas WITH(NOLOCK)
WHERE
	id = @id
