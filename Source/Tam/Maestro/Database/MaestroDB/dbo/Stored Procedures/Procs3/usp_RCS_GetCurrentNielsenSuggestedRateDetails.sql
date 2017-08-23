
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetCurrentNielsenSuggestedRateDetails]
AS
BEGIN
	SELECT
		id,
		nielsen_suggested_rate_id,
		network_id,
		daypart_id,
		suggested_cpm,
		total_spend,
		total_delivery,
		network_group_type
	FROM
		nielsen_suggested_rate_details (NOLOCK)
	WHERE
		nielsen_suggested_rate_id = (
			SELECT MAX(id) FROM nielsen_suggested_rates (NOLOCK)
		)
END
