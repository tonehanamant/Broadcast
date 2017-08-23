CREATE PROCEDURE usp_cmw_traffic_detail_days_update
(
	@cmw_traffic_details_id		Int,
	@day_id		Int,
	@units		Int,
	@is_max		Bit
)
AS
UPDATE cmw_traffic_detail_days SET
	units = @units,
	is_max = @is_max
WHERE
	cmw_traffic_details_id = @cmw_traffic_details_id AND
	day_id = @day_id
