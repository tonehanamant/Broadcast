CREATE PROCEDURE usp_outlook_exports_delete
(
	@id Int
)
AS
DELETE FROM outlook_exports WHERE id=@id
