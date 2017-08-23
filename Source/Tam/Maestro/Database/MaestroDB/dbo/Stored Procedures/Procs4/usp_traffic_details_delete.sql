
-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/06/2015 12:15:41 AM
-- Description:	Auto-generated method to delete a single traffic_details record.
-- =============================================
CREATE PROCEDURE usp_traffic_details_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[traffic_details]
	WHERE
		[id]=@id
END
