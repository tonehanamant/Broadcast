CREATE PROCEDURE [dbo].[usp_preferences_columns_update]
(
	@employee_id		Int,
	@application_code		TinyInt,
	@object_class_code		TinyInt,
	@column_code		TinyInt,
	@visible		Bit,
	@sort_order		TinyInt,
	@column_order		TinyInt,
	@column_width		Int
)
AS
UPDATE dbo.preferences_columns SET
	visible = @visible,
	sort_order = @sort_order,
	column_order = @column_order,
	column_width = @column_width
WHERE
	employee_id = @employee_id AND
	application_code = @application_code AND
	object_class_code = @object_class_code AND
	column_code = @column_code
