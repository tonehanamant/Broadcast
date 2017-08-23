
-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/22/2016 11:49:27 AM
-- Description:	Auto-generated method to delete a single traffic_master_alerts record.
-- =============================================
CREATE PROCEDURE usp_traffic_master_alerts_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[traffic_master_alerts]
	WHERE
		[id]=@id
END

