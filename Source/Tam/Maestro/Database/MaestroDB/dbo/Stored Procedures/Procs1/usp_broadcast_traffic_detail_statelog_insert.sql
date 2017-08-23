CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_statelog_insert]
(
	@id		int		OUTPUT,
	@broadcast_traffic_detail_id		Int,
	@effective_date		DateTime,
	@accepted		Bit,
	@employee_id		Int
)
AS
INSERT INTO broadcast_traffic_detail_statelog
(
	broadcast_traffic_detail_id,
	effective_date,
	accepted,
	employee_id
)
VALUES
(
	@broadcast_traffic_detail_id,
	@effective_date,
	@accepted,
	@employee_id
)

SELECT
	@id = SCOPE_IDENTITY()


