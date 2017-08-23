CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_statelog_delete]
(
	@id Int)
AS
DELETE FROM broadcast_traffic_detail_statelog WHERE id=@id


