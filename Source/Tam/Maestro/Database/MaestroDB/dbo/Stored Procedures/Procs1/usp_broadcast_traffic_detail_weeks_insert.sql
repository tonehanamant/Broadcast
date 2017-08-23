CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_weeks_insert]
(
	@id		int		OUTPUT,
	@broadcast_traffic_detail_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
INSERT INTO broadcast_traffic_detail_weeks
(
	broadcast_traffic_detail_id,
	start_date,
	end_date,
	selected
)
VALUES
(
	@broadcast_traffic_detail_id,
	@start_date,
	@end_date,
	@selected
)

SELECT
	@id = SCOPE_IDENTITY()


