-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/25/2011
-- Description:	
-- =============================================
-- usp_RCS_GetNetworkRateCardBookBusinessObject 1
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardBookBusinessObject] 
	@network_rate_card_book_id INT
AS
BEGIN
	-- rate cards
	SELECT
		id,
		network_rate_card_book_id,
		rate_card_type_id,
		daypart_id
	FROM
		network_rate_cards (NOLOCK)
	WHERE
		network_rate_card_book_id=@network_rate_card_book_id

	-- rate card details
    SELECT
		nrcd.id,
		nrcd.network_rate_card_id,
		nrcd.tier,
		nrcd.network_id,
		nrcd.primary_audience_id,
		nrcd.hh_us_universe,
		nrcd.hh_coverage_universe,
		nrcd.hh_cpm,
		nrcd.hh_rating,
		nrcd.hh_delivery,
		nrcd.hh_net_cost_cpm,
		nrcd.demo_us_universe,
		nrcd.demo_coverage_universe,
		nrcd.demo_cpm,
		nrcd.demo_rating,
		nrcd.demo_delivery,
		nrcd.demo_net_cost_cpm,
		nrcd.lock_rate,
		nrcd.minimum_cpm,
		networks.code,
		a.name
	FROM
		network_rate_card_details nrcd (NOLOCK)
		JOIN networks (NOLOCK) ON networks.id=nrcd.network_id
		LEFT JOIN audiences a (NOLOCK) ON a.id=nrcd.primary_audience_id
	WHERE
		nrcd.network_rate_card_id IN (
			SELECT id FROM network_rate_cards (NOLOCK) WHERE network_rate_card_book_id=@network_rate_card_book_id
		)
	ORDER BY
		networks.code

	-- rate card detail rates
	SELECT
		id,
		network_rate_card_detail_id,
		spot_length_id,
		rate
	FROM
		network_rate_card_rates (NOLOCK)
	WHERE
		network_rate_card_detail_id IN (
			SELECT
				id
			FROM
				network_rate_card_details (NOLOCK)
			WHERE 
				network_rate_card_id IN (
					SELECT id FROM network_rate_cards (NOLOCK) WHERE network_rate_card_book_id=@network_rate_card_book_id
				)
		)

	-- topographies
	SELECT
		network_rate_card_book_id,
		topography_id
	FROM
		network_rate_card_book_topographies (NOLOCK)
	WHERE
		network_rate_card_book_id=@network_rate_card_book_id
END
