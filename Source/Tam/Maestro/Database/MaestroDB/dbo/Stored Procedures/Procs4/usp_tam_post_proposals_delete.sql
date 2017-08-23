-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/19/2014 02:08:43 PM
-- Description:	Auto-generated method to delete a single tam_post_proposals record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_proposals_delete]
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[tam_post_proposals]
	WHERE
		[id]=@id
END

