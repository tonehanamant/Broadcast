-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/16/2009
-- Modified:	1/17/2011
-- Description:	Feeds the data for the rate card comparison report.
-- Changes:		1/17/2011	Added @effective_date variable to use in the joining of networks to get the appropriate code.
-- =============================================
-- usp_RCS_GetRateCardComparisonReport 79,1,1
CREATE PROCEDURE [dbo].[usp_RCS_GetRateCardComparisonReport]
	@network_rate_card_book_id INT,
	@daypart_id INT,
	@rate_card_type_id INT
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
		nrcb.name,
		'Q' + CAST(nrcb.quarter AS VARCHAR(1)) + '''' + SUBSTRING(CAST(nrcb.[year] AS VARCHAR(4)),3,2),
		d.name,
		d.daypart_text,
		rct.name 'rate_card_type'
	FROM
		network_rate_cards nrc				(NOLOCK)
		JOIN network_rate_card_books nrcb	(NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
		JOIN vw_ccc_daypart d				(NOLOCK) ON d.id=nrc.daypart_id
		JOIN rate_card_types rct			(NOLOCK) ON rct.id=nrc.rate_card_type_id
	WHERE
		nrcb.id=@network_rate_card_book_id
		AND nrc.daypart_id=@daypart_id
		AND nrc.rate_card_type_id=@rate_card_type_id

	SELECT
		nrcd.tier,
		n.code,
		nrcd.hh_coverage_universe / 1000.0 'hh_cov_ue',
		nrcr.rate,
		nrcd.hh_rating * 100.0 'hh_rtg',
		nrcd.hh_delivery / 1000.0 'hh_delivery',
		nrcd.hh_cpm
	FROM
		network_rate_card_rates nrcr
		JOIN network_rate_card_details nrcd (NOLOCK) ON nrcd.id=nrcr.network_rate_card_detail_id
		JOIN network_rate_cards nrc			(NOLOCK) ON nrc.id=nrcd.network_rate_card_id
		JOIN network_rate_card_books nrcb	(NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
		JOIN uvw_network_universe n			(NOLOCK) ON n.network_id=nrcd.network_id AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
	WHERE
		nrcb.id=@network_rate_card_book_id
		AND nrc.daypart_id=@daypart_id
		AND nrc.rate_card_type_id=@rate_card_type_id
		AND nrcr.spot_length_id=1
	ORDER BY
		nrcd.tier,
		n.code
END


/****** Object:  StoredProcedure [dbo].[usp_RCS_QueryNetworkRateCardDetails]    Script Date: 01/17/2011 09:48:43 ******/
SET ANSI_NULLS ON
