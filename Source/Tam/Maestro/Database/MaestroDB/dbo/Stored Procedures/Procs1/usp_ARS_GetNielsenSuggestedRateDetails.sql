-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetNielsenSuggestedRateDetails]
	@year INT,
	@quarter INT,
	@media_month_id INT
AS
BEGIN
	SELECT
		nielsen_suggested_rate_details.id,
		nielsen_suggested_rate_details.nielsen_suggested_rate_id,
		nielsen_suggested_rate_details.network_id,
		nielsen_suggested_rate_details.daypart_id,
		nielsen_suggested_rate_details.suggested_cpm,
		nielsen_suggested_rate_details.total_spend,
		nielsen_suggested_rate_details.total_delivery,
		nielsen_suggested_rate_details.network_group_type
	FROM
		nielsen_suggested_rate_details
		JOIN nielsen_suggested_rates ON nielsen_suggested_rates.id=nielsen_suggested_rate_details.nielsen_suggested_rate_id
	WHERE
		nielsen_suggested_rates.year=@year
		AND nielsen_suggested_rates.quarter=@quarter
		AND ((@media_month_id IS NULL AND nielsen_suggested_rates.media_month_id IS NULL) OR nielsen_suggested_rates.media_month_id=@media_month_id)
END