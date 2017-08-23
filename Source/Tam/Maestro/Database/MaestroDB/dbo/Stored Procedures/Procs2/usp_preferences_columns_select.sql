CREATE PROCEDURE [dbo].[usp_preferences_columns_select]
(
	@employee_id		Int,
	@application_code		TinyInt,
	@object_class_code		TinyInt,
	@column_code		TinyInt
)
AS
SELECT
	*
FROM
	dbo.preferences_columns WITH(NOLOCK)
WHERE
	employee_id=@employee_id
	AND
	application_code=@application_code
	AND
	object_class_code=@object_class_code
	AND
	column_code=@column_code
