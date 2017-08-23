CREATE PROCEDURE usp_statuses_delete
(
	@id Int
)
AS
DELETE FROM statuses WHERE id=@id
