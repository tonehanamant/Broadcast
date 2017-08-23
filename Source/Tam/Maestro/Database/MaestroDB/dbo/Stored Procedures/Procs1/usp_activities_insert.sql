CREATE PROCEDURE usp_activities_insert
(
	@id		Int		OUTPUT,
	@contact_id		Int,
	@activity_type_id		Int,
	@employee_id		Int,
	@comment		Text,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO activities
(
	contact_id,
	activity_type_id,
	employee_id,
	comment,
	date_created,
	date_last_modified
)
VALUES
(
	@contact_id,
	@activity_type_id,
	@employee_id,
	@comment,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

