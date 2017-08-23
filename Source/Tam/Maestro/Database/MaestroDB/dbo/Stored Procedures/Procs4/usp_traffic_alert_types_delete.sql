CREATE PROCEDURE usp_traffic_alert_types_delete
(
	@id Int)
AS
DELETE FROM traffic_alert_types WHERE id=@id
