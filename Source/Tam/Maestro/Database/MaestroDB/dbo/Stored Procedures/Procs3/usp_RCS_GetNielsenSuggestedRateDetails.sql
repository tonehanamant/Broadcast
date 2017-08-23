
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetNielsenSuggestedRateDetails]
	@year INT,
	@quarter INT,
	@media_month_id INT
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
		nielsen_suggested_rate_id IN (
			SELECT 
				id 
			FROM 
				nielsen_suggested_rates (NOLOCK)
			WHERE 
				year=@year 
				AND quarter=@quarter 
				AND ((@media_month_id IS NULL AND media_month_id IS NULL) OR media_month_id=@media_month_id)
		)
END
