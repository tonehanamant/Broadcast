CREATE PROCEDURE usp_activities_update
(
	@id		Int,
	@contact_id		Int,
	@activity_type_id		Int,
	@employee_id		Int,
	@comment		Text,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE activities SET
	contact_id = @contact_id,
	activity_type_id = @activity_type_id,
	employee_id = @employee_id,
	comment = @comment,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

