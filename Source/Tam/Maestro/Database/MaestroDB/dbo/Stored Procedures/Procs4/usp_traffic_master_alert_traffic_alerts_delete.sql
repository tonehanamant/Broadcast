CREATE PROCEDURE [dbo].[usp_traffic_master_alert_traffic_alerts_delete]
(
	@id Int)
AS
DELETE FROM traffic_master_alert_traffic_alerts WHERE id=@id
