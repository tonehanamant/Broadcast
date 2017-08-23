-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/11/2013 02:33:52 PM
-- Description:	Auto-generated method to insert a rating_category_groups record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_rating_category_groups_insert]
	@id TINYINT OUTPUT,
	@name VARCHAR(63)
AS
BEGIN
	INSERT INTO [dbo].[rating_category_groups]
	(
		[name]
	)
	VALUES
	(
		@name
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
