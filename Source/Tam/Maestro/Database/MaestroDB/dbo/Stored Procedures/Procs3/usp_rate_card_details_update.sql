
CREATE PROCEDURE [dbo].[usp_rate_card_details_update]
(
	@id		Int,
	@rate_card_id		Int,
	@network_id		Int,
	@tier		Int,
	@primary_audience_id		Int,
	@rate_15		Money,
	@rate_30		Money,
	@rate_60		Money,
	@net_cost_hh_cpm		Money,
	@net_cost_demo_cpm		Money
)
AS
UPDATE rate_card_details SET
	rate_card_id = @rate_card_id,
	network_id = @network_id,
	tier = @tier,
	primary_audience_id = @primary_audience_id,
	rate_15 = @rate_15,
	rate_30 = @rate_30,
	rate_60 = @rate_60,
	net_cost_hh_cpm = @net_cost_hh_cpm,
	net_cost_demo_cpm = @net_cost_demo_cpm
WHERE
	id = @id
