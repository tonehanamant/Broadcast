CREATE PROCEDURE [dbo].[usp_broadcast_traffic_details_delete]
(
	@id Int
)
AS
DELETE FROM broadcast_traffic_details WHERE id=@id


