CREATE PROCEDURE usp_days_delete
(
	@id Int
)
AS
DELETE FROM days WHERE id=@id
