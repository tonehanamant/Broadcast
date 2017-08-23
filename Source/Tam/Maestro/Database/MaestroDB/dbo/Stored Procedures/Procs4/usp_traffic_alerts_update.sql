
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/23/2016 01:34:03 PM
-- Description:	Auto-generated method to update a traffic_alerts record.
-- =============================================
CREATE PROCEDURE usp_traffic_alerts_update
	@id INT,
	@alert_comment VARCHAR(255),
	@traffic_id INT,
	@traffic_alert_type_id INT,
	@copy_comment VARCHAR(255),
	@effective_date DATETIME,
	@last_modified_date DATETIME,
	@last_modified_user INT
AS
BEGIN
	UPDATE
		[dbo].[traffic_alerts]
	SET
		[alert_comment]=@alert_comment,
		[traffic_id]=@traffic_id,
		[traffic_alert_type_id]=@traffic_alert_type_id,
		[copy_comment]=@copy_comment,
		[effective_date]=@effective_date,
		[last_modified_date]=@last_modified_date,
		[last_modified_user]=@last_modified_user
	WHERE
		[id]=@id
END
