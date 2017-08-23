
-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/22/2016 11:49:27 AM
-- Description:	Auto-generated method to update a traffic_master_alerts record.
-- =============================================
CREATE PROCEDURE usp_traffic_master_alerts_update
	@id INT,
	@name VARCHAR(63),
	@date_created DATETIME,
	@status_id INT
AS
BEGIN
	UPDATE
		[dbo].[traffic_master_alerts]
	SET
		[name]=@name,
		[date_created]=@date_created,
		[status_id]=@status_id
	WHERE
		[id]=@id
END
