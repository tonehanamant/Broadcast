CREATE PROCEDURE usp_statements_delete
(
	@id Int
)
AS
DELETE FROM statements WHERE id=@id
