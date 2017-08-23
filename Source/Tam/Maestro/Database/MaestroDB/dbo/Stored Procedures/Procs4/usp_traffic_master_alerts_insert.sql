
-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/22/2016 11:49:27 AM
-- Description:	Auto-generated method to insert a traffic_master_alerts record.
-- =============================================
CREATE PROCEDURE usp_traffic_master_alerts_insert
	@id INT OUTPUT,
	@name VARCHAR(63),
	@date_created DATETIME,
	@status_id INT
AS
BEGIN
	INSERT INTO [dbo].[traffic_master_alerts]
	(
		[name],
		[date_created],
		[status_id]
	)
	VALUES
	(
		@name,
		@date_created,
		@status_id
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
