CREATE PROCEDURE usp_audiences_delete
(
	@id Int
)
AS
DELETE FROM audiences WHERE id=@id
