CREATE PROCEDURE usp_system_custom_traffic_delete
(
	@system_id		Int
)
AS
DELETE FROM system_custom_traffic WHERE system_id=@system_id
