CREATE PROCEDURE usp_documents_select_all
AS
SELECT
	*
FROM
	documents WITH(NOLOCK)
