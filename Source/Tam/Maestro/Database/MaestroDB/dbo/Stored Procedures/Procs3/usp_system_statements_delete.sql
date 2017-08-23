CREATE PROCEDURE usp_system_statements_delete
(
	@id Int
)
AS
DELETE FROM system_statements WHERE id=@id
