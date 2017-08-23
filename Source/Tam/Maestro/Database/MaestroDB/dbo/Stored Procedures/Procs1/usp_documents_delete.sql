CREATE PROCEDURE usp_documents_delete
(
	@id Int
)
AS
DELETE FROM documents WHERE id=@id
