CREATE PROCEDURE usp_notes_delete
(
	@id Int
)
AS
DELETE FROM notes WHERE id=@id
