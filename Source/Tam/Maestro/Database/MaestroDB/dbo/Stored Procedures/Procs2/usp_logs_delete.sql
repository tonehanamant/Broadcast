CREATE PROCEDURE usp_logs_delete
(
	@id Int
)
AS
DELETE FROM logs WHERE id=@id
