-- =============================================
-- Author:		CRUD Creator
-- Create date: 03/25/2016 01:41:05 PM
-- Description:	Auto-generated method to delete a single proposals record.
-- =============================================
CREATE PROCEDURE usp_proposals_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[proposals]
	WHERE
		[id]=@id
END
