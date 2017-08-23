
CREATE PROCEDURE [dbo].[usp_REL_PurgeTrafficOrdersByReleaseAndSystemID]
(
	@release_id Int,
	@system_id int
)
AS

delete from traffic_orders where release_id = @release_id and system_id = @system_id

