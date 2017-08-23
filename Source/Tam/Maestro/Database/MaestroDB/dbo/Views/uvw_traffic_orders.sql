CREATE VIEW [dbo].[uvw_traffic_orders]
AS
SELECT
	id,
	system_id,
	zone_id,
	traffic_detail_id,
	daypart_id,
	ordered_spots,
	CASE WHEN start_date < '12/12/2014' THEN ordered_spot_rate ELSE NULL END 'ordered_spot_rate_net',
	CASE WHEN start_date >= '12/12/2014' THEN ordered_spot_rate ELSE NULL END 'ordered_spot_rate_gross',
	start_date,
	end_date,
	release_id,
	subscribers,
	display_network_id,
	on_financial_reports,
	active
FROM
	traffic_orders tro (NOLOCK)
