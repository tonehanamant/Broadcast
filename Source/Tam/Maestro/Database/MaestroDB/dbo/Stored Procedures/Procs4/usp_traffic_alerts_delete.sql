-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/23/2016 01:34:03 PM
-- Description:	Auto-generated method to delete a single traffic_alerts record.
-- =============================================
CREATE PROCEDURE usp_traffic_alerts_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[traffic_alerts]
	WHERE
		[id]=@id
END
