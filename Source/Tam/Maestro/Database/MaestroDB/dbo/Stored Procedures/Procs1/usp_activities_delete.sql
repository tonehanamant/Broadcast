CREATE PROCEDURE usp_activities_delete
(
	@id Int
)
AS
DELETE FROM activities WHERE id=@id
