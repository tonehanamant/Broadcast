-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_GetAffidavitsToCalculateHash]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
	SET ROWCOUNT 100000

	SELECT
		a.id, 
		a.air_date, 
		a.air_time, 
		sl.length,
		m.code,
		n.code,
		a.rate,
		z.code
	FROM
		affidavits a					(NOLOCK)
		JOIN spot_lengths sl			(NOLOCK) ON sl.id=a.spot_length_id
		JOIN materials m				(NOLOCK) ON m.id=a.material_id
		JOIN uvw_network_universe n		(NOLOCK) ON n.network_id=a.network_id	AND (n.start_date<=a.air_date AND (n.end_date>=a.air_date OR n.end_date IS NULL))
		JOIN uvw_zone_universe z		(NOLOCK) ON z.zone_id=a.zone_id			AND (z.start_date<=a.air_date AND (z.end_date>=a.air_date OR z.end_date IS NULL))
	WHERE
		a.media_month_id=@media_month_id
		AND a.material_id IS NOT NULL
		AND a.zone_id IS NOT NULL
		AND a.network_id IS NOT NULL
		AND a.spot_length_id IS NOT NULL
		AND a.air_date IS NOT NULL
		AND a.air_time IS NOT NULL
		AND a.rate IS NOT NULL
		AND a.hash IS NULL
END
