-- =============================================
-- Author:		CRUD Creator
-- Create date: 01/13/2014 11:12:40 AM
-- Description:	Auto-generated method to insert a preferences_columns record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_preferences_columns_insert]
	@employee_id INT,
	@application_code TINYINT,
	@object_class_code TINYINT,
	@column_code TINYINT,
	@visible BIT,
	@sort_order TINYINT,
	@column_order TINYINT,
	@column_width INT
AS
BEGIN
	INSERT INTO [dbo].[preferences_columns]
	(
		[employee_id],
		[application_code],
		[object_class_code],
		[column_code],
		[visible],
		[sort_order],
		[column_order],
		[column_width]
	)
	VALUES
	(
		@employee_id,
		@application_code,
		@object_class_code,
		@column_code,
		@visible,
		@sort_order,
		@column_order,
		@column_width
	)
END
