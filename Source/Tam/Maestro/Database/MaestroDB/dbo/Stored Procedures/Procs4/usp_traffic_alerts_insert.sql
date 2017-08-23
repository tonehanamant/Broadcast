
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/23/2016 01:34:03 PM
-- Description:	Auto-generated method to insert a traffic_alerts record.
-- =============================================
CREATE PROCEDURE usp_traffic_alerts_insert
	@id INT OUTPUT,
	@alert_comment VARCHAR(255),
	@traffic_id INT,
	@traffic_alert_type_id INT,
	@copy_comment VARCHAR(255),
	@effective_date DATETIME,
	@last_modified_date DATETIME,
	@last_modified_user INT
AS
BEGIN
	INSERT INTO [dbo].[traffic_alerts]
	(
		[alert_comment],
		[traffic_id],
		[traffic_alert_type_id],
		[copy_comment],
		[effective_date],
		[last_modified_date],
		[last_modified_user]
	)
	VALUES
	(
		@alert_comment,
		@traffic_id,
		@traffic_alert_type_id,
		@copy_comment,
		@effective_date,
		@last_modified_date,
		@last_modified_user
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
