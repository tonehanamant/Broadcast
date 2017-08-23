CREATE PROCEDURE [dbo].[usp_preferences_columns_delete]
(
	@employee_id		Int,
	@application_code		TinyInt,
	@object_class_code		TinyInt,
	@column_code		TinyInt)
AS
DELETE FROM
	dbo.preferences_columns
WHERE
	employee_id = @employee_id
 AND
	application_code = @application_code
 AND
	object_class_code = @object_class_code
 AND
	column_code = @column_code
