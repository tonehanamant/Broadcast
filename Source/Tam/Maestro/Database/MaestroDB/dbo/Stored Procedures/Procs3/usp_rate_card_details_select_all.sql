
CREATE PROCEDURE [dbo].[usp_rate_card_details_select_all]
AS
SELECT
	id,
	rate_card_id,
	network_id,
	tier,
	primary_audience_id,
	rate_15,
	rate_30,
	rate_60,
	net_cost_hh_cpm,
	net_cost_demo_cpm
FROM
	rate_card_details
