CREATE PROCEDURE usp_activity_types_delete
(
	@id Int
)
AS
DELETE FROM activity_types WHERE id=@id
