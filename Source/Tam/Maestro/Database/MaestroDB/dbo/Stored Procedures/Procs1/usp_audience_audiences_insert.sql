-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/11/2013 02:33:49 PM
-- Description:	Auto-generated method to insert a audience_audiences record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_audience_audiences_insert]
	@rating_category_group_id TINYINT,
	@custom_audience_id INT,
	@rating_audience_id INT
AS
BEGIN
	INSERT INTO [dbo].[audience_audiences]
	(
		[rating_category_group_id],
		[custom_audience_id],
		[rating_audience_id]
	)
	VALUES
	(
		@rating_category_group_id,
		@custom_audience_id,
		@rating_audience_id
	)
END
