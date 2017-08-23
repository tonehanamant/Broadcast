-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:55 AM
-- Description:	Auto-generated method to delete a single regions_histories record.
-- =============================================
create PROCEDURE dbo.usp_regions_histories_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[regions_histories]
	WHERE
		[id]=@id
END