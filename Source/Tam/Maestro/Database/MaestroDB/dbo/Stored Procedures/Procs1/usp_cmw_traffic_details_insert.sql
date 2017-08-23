CREATE PROCEDURE usp_cmw_traffic_details_insert
(
	@id		Int		OUTPUT,
	@cmw_traffic_id		Int,
	@daypart_id		Int,
	@spot_length_id		Int,
	@unit_cost		Money,
	@total_units		Int,
	@suspend_date		DateTime,
	@start_date		DateTime,
	@end_date		DateTime,
	@original_cmw_traffic_detail_id		Int,
	@status_code		TinyInt,
	@line_number		Decimal(18,2)
)
AS
INSERT INTO cmw_traffic_details
(
	cmw_traffic_id,
	daypart_id,
	spot_length_id,
	unit_cost,
	total_units,
	suspend_date,
	start_date,
	end_date,
	original_cmw_traffic_detail_id,
	status_code,
	line_number
)
VALUES
(
	@cmw_traffic_id,
	@daypart_id,
	@spot_length_id,
	@unit_cost,
	@total_units,
	@suspend_date,
	@start_date,
	@end_date,
	@original_cmw_traffic_detail_id,
	@status_code,
	@line_number
)

SELECT
	@id = SCOPE_IDENTITY()

