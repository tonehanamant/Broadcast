-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/8/2012
-- Description:	<Description,,>
-- =============================================
-- usp_STS2_GetFullCoverageUniverseByNetworkRateCardBook 353,383
CREATE PROCEDURE [dbo].[usp_STS2_GetFullCoverageUniverseByNetworkRateCardBook]
	@network_rate_card_book_id INT,
	@base_media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		cud.network_id,
		SUM(cud.universe) 'universe'
	FROM
		coverage_universes cu (NOLOCK)
		JOIN coverage_universe_details cud (NOLOCK) ON cud.coverage_universe_id=cu.id
		JOIN network_rate_card_book_topographies nrcbt (NOLOCK) ON nrcbt.topography_id=cud.topography_id
			AND nrcbt.network_rate_card_book_id=@network_rate_card_book_id
		JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=nrcbt.network_rate_card_book_id
	WHERE
		cu.base_media_month_id=@base_media_month_id
		AND cu.sales_model_id=nrcb.sales_model_id
	GROUP BY
		cud.network_id
END
