-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Modified:	1/6/2011
-- Description:	Feeds the rate card report.
-- Changes:		1/6/2011	Changes the order by clause to sort by network rather than tier and network.
--				1/17/2011	Added @effective_date variable to use in the joining of networks to get the appropriate code.
-- =============================================
-- usp_RCS_GetNetworkRateCardReport 165, 1
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardReport]
	@network_rate_card_book_id INT,
	@spot_length_id INT
AS
BEGIN
	DECLARE @effective_date DATETIME
	SET @effective_date = (
		SELECT 
			MIN(mm.start_date) 
		FROM 
			network_rate_card_books nrcb (NOLOCK) 
			JOIN media_months mm (NOLOCK) ON mm.[year]=nrcb.[year] 
				AND CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = nrcb.quarter
				AND (nrcb.media_month_id IS NULL OR mm.id=nrcb.media_month_id)
		WHERE
			nrcb.id=@network_rate_card_book_id
	)

	SELECT
		':' + CAST(sl.length AS VARCHAR) + ' ' + nrcb.name + ': ' + rct.name + ': ' + d.name + ': ' + d.daypart_text,
		n.code,
		nrcd.tier,
		nrcr.rate,
		nrcd.hh_us_universe,
		nrcd.hh_coverage_universe,
		nrcd.hh_rating,
		(nrcd.hh_delivery * sl.delivery_multiplier) / 1000.0 [hh_delivery],
		CASE WHEN nrcd.hh_delivery > 0 THEN CAST(nrcr.rate / ((nrcd.hh_delivery * sl.delivery_multiplier) / 1000.0) AS MONEY) ELSE 0 END [hh_cpm],
		nrcd.demo_us_universe,
		nrcd.demo_coverage_universe,
		nrcd.demo_rating,
		(nrcd.demo_delivery * sl.delivery_multiplier) / 1000.0 [demo_delivery],
		CASE WHEN nrcd.demo_delivery > 0 THEN CAST(nrcr.rate / ((nrcd.demo_delivery * sl.delivery_multiplier) / 1000.0) AS MONEY) ELSE 0 END [demo_cpm],
		a.code
	FROM
		network_rate_card_rates nrcr (NOLOCK)
		JOIN network_rate_card_details nrcd (NOLOCK) ON nrcd.id=nrcr.network_rate_card_detail_id
		JOIN network_rate_cards nrc (NOLOCK) ON nrc.id=nrcd.network_rate_card_id
		JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
		JOIN uvw_network_universe n (NOLOCK) ON n.network_id=nrcd.network_id AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
		JOIN dayparts d (NOLOCK) ON d.id=nrc.daypart_id
		JOIN rate_card_types rct (NOLOCK) ON rct.id=nrc.rate_card_type_id
		JOIN spot_lengths sl (NOLOCK) ON sl.id=@spot_length_id
		LEFT JOIN audiences a (NOLOCK) ON a.id=nrcd.primary_audience_id
	WHERE
		nrcb.id=@network_rate_card_book_id
		AND nrcr.spot_length_id=@spot_length_id
	ORDER BY
		nrcb.name,
		rct.name,
		d.name,
		d.daypart_text,
		n.code
END
