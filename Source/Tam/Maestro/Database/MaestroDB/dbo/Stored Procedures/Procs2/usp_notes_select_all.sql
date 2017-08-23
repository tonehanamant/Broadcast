CREATE PROCEDURE usp_notes_select_all
AS
SELECT
	*
FROM
	notes WITH(NOLOCK)
