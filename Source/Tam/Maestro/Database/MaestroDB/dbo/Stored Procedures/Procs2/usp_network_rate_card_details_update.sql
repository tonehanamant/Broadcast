
CREATE PROCEDURE usp_network_rate_card_details_update
(
	@id		Int,
	@network_rate_card_id		Int,
	@tier		Int,
	@network_id		Int,
	@primary_audience_id		Int,
	@hh_us_universe		Float,
	@hh_coverage_universe		Float,
	@hh_cpm		Money,
	@hh_rating		Float,
	@hh_delivery		Float,
	@hh_net_cost_cpm		Money,
	@demo_us_universe		Float,
	@demo_coverage_universe		Float,
	@demo_cpm		Money,
	@demo_rating		Float,
	@demo_delivery		Float,
	@demo_net_cost_cpm		Money,
	@lock_rate		Bit,
	@minimum_cpm		Money
)
AS
BEGIN
UPDATE dbo.network_rate_card_details SET
	network_rate_card_id = @network_rate_card_id,
	tier = @tier,
	network_id = @network_id,
	primary_audience_id = @primary_audience_id,
	hh_us_universe = @hh_us_universe,
	hh_coverage_universe = @hh_coverage_universe,
	hh_cpm = @hh_cpm,
	hh_rating = @hh_rating,
	hh_delivery = @hh_delivery,
	hh_net_cost_cpm = @hh_net_cost_cpm,
	demo_us_universe = @demo_us_universe,
	demo_coverage_universe = @demo_coverage_universe,
	demo_cpm = @demo_cpm,
	demo_rating = @demo_rating,
	demo_delivery = @demo_delivery,
	demo_net_cost_cpm = @demo_net_cost_cpm,
	lock_rate = @lock_rate,
	minimum_cpm = @minimum_cpm
WHERE
	id = @id

END
