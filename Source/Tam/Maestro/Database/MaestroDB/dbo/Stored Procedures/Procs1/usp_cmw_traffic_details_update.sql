CREATE PROCEDURE usp_cmw_traffic_details_update
(
	@id		Int,
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
UPDATE cmw_traffic_details SET
	cmw_traffic_id = @cmw_traffic_id,
	daypart_id = @daypart_id,
	spot_length_id = @spot_length_id,
	unit_cost = @unit_cost,
	total_units = @total_units,
	suspend_date = @suspend_date,
	start_date = @start_date,
	end_date = @end_date,
	original_cmw_traffic_detail_id = @original_cmw_traffic_detail_id,
	status_code = @status_code,
	line_number = @line_number
WHERE
	id = @id

