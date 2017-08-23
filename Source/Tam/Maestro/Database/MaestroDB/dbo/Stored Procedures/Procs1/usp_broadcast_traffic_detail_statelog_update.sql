CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_statelog_update]
(
	@id		Int,
	@broadcast_traffic_detail_id		Int,
	@effective_date		DateTime,
	@accepted		Bit,
	@employee_id		Int
)
AS
UPDATE broadcast_traffic_detail_statelog SET
	broadcast_traffic_detail_id = @broadcast_traffic_detail_id,
	effective_date = @effective_date,
	accepted = @accepted,
	employee_id = @employee_id
WHERE
	id = @id


