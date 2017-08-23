
-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/01/2015 12:40:31 PM
-- Description:	Auto-generated method to delete a single tam_posts record.
-- =============================================
CREATE PROCEDURE usp_tam_posts_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[tam_posts]
	WHERE
		[id]=@id
END
