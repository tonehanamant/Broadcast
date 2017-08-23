
-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/08/2015 01:46:43 PM
-- Description:	Auto-generated method to delete a single traffic record.
-- =============================================
CREATE PROCEDURE usp_traffic_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[traffic]
	WHERE
		[id]=@id
END
