CREATE PROCEDURE usp_notes_select
(
	@id Int
)
AS
SELECT
	*
FROM
	notes WITH(NOLOCK)
WHERE
	id = @id
