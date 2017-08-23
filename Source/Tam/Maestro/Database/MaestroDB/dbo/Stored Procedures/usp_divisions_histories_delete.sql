-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:54 AM
-- Description:	Auto-generated method to delete a single divisions_histories record.
-- =============================================
create PROCEDURE dbo.usp_divisions_histories_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[divisions_histories]
	WHERE
		[id]=@id
END