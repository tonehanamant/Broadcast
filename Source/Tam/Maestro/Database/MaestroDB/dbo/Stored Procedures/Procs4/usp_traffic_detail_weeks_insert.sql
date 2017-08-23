CREATE PROCEDURE usp_traffic_detail_weeks_insert
(
	@id		int		OUTPUT,
	@traffic_detail_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@suspended		Bit
)
AS
INSERT INTO traffic_detail_weeks
(
	traffic_detail_id,
	start_date,
	end_date,
	suspended
)
VALUES
(
	@traffic_detail_id,
	@start_date,
	@end_date,
	@suspended
)

SELECT
	@id = SCOPE_IDENTITY()

