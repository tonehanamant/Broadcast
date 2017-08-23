CREATE PROCEDURE usp_topographies_select
(
	@id Int
)
AS
SELECT
	*
FROM
	topographies WITH(NOLOCK)
WHERE
	id = @id
