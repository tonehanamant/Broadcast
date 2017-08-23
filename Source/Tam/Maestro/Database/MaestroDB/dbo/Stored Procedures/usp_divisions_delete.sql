-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:54 AM
-- Description:	Auto-generated method to delete a single divisions record.
-- =============================================
create PROCEDURE dbo.usp_divisions_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[divisions]
	WHERE
		[id]=@id
END