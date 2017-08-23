-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/21/2014 02:22:12 PM
-- Description:	Auto-generated method to insert a tam_post_plan_types record.
-- =============================================
CREATE PROCEDURE usp_tam_post_plan_types_insert
	@id TINYINT,
	@plan_type_name VARCHAR(100)
AS
BEGIN
BEGIN
	INSERT INTO [dbo].[tam_post_plan_types]
	(
		[id],
		[plan_type_name]
	)
	VALUES
	(
		@id,
		@plan_type_name
	)
END
END
