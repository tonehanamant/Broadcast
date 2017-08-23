-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Modified:	1/17/2011
-- Description:	Retrieves a rate card and details lines.
-- Changes:		1/17/2011	Added @effective_date variable to use in the joining of networks to get the appropriate code.
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetDisplayNetworkRateCardDetail]
	@network_rate_card_detail_id INT
AS
BEGIN
	DECLARE @effective_date DATETIME
	SET @effective_date = (
		SELECT 
			MIN(mm.start_date) 
		FROM 
			network_rate_card_details nrcd (NOLOCK)
			JOIN network_rate_cards nrc (NOLOCK) ON nrc.id=nrcd.network_rate_card_id
			JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
			JOIN media_months mm (NOLOCK) ON mm.[year]=nrcb.[year] 
				AND CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = nrcb.quarter
				AND (nrcb.media_month_id IS NULL OR mm.id=nrcb.media_month_id)
		WHERE
			nrcd.id=@network_rate_card_detail_id
	)

    SELECT
		nrcd.id,
		rate_card_types.name,
		dayparts.daypart_text,
		n.code,
		nrcd.tier,
		a.name,
		nrcd.hh_us_universe, 
		nrcd.hh_coverage_universe, 
		nrcd.hh_cpm, hh_rating, 
		nrcd.hh_delivery, 
		nrcd.demo_us_universe, 
		nrcd.demo_coverage_universe, 		
		nrcd.demo_cpm, 
		nrcd.demo_rating, 
		nrcd.demo_delivery,
		nrc.id
	FROM
		network_rate_card_details nrcd (NOLOCK)
		JOIN network_rate_cards nrc (NOLOCK) ON nrc.id=nrcd.network_rate_card_id
		JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
		JOIN uvw_network_universe n (NOLOCK) ON n.network_id=nrcd.network_id AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
		JOIN dayparts (NOLOCK) ON dayparts.id=nrc.daypart_id
		JOIN rate_card_types (NOLOCK) ON rate_card_types.id=nrc.rate_card_type_id
		LEFT JOIN audiences a (NOLOCK) ON a.id=nrcd.primary_audience_id
	WHERE
		nrcd.id=@network_rate_card_detail_id
	ORDER BY
		n.code


	SELECT
		nrcr.network_rate_card_detail_id,
		sl.length,
		nrcr.rate
	FROM
		network_rate_card_rates nrcr (NOLOCK)
		JOIN spot_lengths sl (NOLOCK) ON sl.id=nrcr.spot_length_id
	WHERE
		network_rate_card_detail_id=@network_rate_card_detail_id
END
