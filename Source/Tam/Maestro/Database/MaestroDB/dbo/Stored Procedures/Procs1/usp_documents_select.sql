CREATE PROCEDURE usp_documents_select
(
	@id Int
)
AS
SELECT
	*
FROM
	documents WITH(NOLOCK)
WHERE
	id = @id
