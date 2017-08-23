CREATE PROCEDURE [dbo].[usp_rate_card_details_insert]
(
	@id		int		OUTPUT,
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
INSERT INTO rate_card_details
(
	rate_card_id,
	network_id,
	tier,
	primary_audience_id,
	rate_15,
	rate_30,
	rate_60,
	net_cost_hh_cpm,
	net_cost_demo_cpm
)
VALUES
(
	@rate_card_id,
	@network_id,
	@tier,
	@primary_audience_id,
	@rate_15,
	@rate_30,
	@rate_60,
	@net_cost_hh_cpm,
	@net_cost_demo_cpm
)

SELECT
	@id = @@Identity
