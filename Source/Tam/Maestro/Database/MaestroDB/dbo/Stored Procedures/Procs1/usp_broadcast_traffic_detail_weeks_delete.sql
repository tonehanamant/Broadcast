CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_weeks_delete]
(
	@id Int)
AS
DELETE FROM broadcast_traffic_detail_weeks WHERE id=@id


