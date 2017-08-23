-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:55 AM
-- Description:	Auto-generated method to delete a single regions record.
-- =============================================
create PROCEDURE dbo.usp_regions_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[regions]
	WHERE
		[id]=@id
END