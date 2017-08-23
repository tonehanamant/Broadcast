-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_GetAffidavitsToCalculateSubscribers]
	@media_month_id INT,
	@last_id BIGINT
AS
BEGIN
	SET ROWCOUNT 100000

	SELECT
		a.*
	FROM 
		affidavits a (NOLOCK)
		LEFT JOIN uvw_zonenetwork_universe zn (NOLOCK) ON 
			zn.zone_id=a.zone_id 
			AND zn.network_id=a.network_id 
			AND (zn.start_date<=a.air_date AND (zn.end_date>=a.air_date OR zn.end_date IS NULL))
	WHERE
		a.media_month_id=@media_month_id
		AND a.zone_id IS NOT NULL
		AND a.network_id IS NOT NULL
		AND a.air_date IS NOT NULL
		AND (a.subscribers IS NULL OR a.subscribers = 0)
		AND zn.subscribers IS NOT NULL
		AND zn.subscribers > 0
		AND (@last_id IS NULL OR a.id>@last_id)
END
