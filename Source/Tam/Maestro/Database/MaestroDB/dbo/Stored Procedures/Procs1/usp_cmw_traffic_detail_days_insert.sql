CREATE PROCEDURE usp_cmw_traffic_detail_days_insert
(
	@cmw_traffic_details_id		Int,
	@day_id		Int,
	@units		Int,
	@is_max		Bit
)
AS
INSERT INTO cmw_traffic_detail_days
(
	cmw_traffic_details_id,
	day_id,
	units,
	is_max
)
VALUES
(
	@cmw_traffic_details_id,
	@day_id,
	@units,
	@is_max
)

