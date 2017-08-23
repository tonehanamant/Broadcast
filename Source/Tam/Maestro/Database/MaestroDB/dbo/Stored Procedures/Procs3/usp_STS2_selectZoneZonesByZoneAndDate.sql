-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/11/2011
-- Description:	
-- =============================================
-- usp_STS2_selectZoneZonesByZoneAndDate 3858,'8/1/2011'
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneZonesByZoneAndDate]
	@zone_id int,
	@effective_date DATETIME
AS
BEGIN
	SELECT
		zz.primary_zone_id,
		zz.secondary_zone_id,
		zz.type,
		zz.start_date
	FROM
		uvw_zonezone_universe zz (NOLOCK)
	WHERE
		zz.primary_zone_id=@zone_id
		AND (zz.start_date<=@effective_date AND (zz.end_date>=@effective_date OR zz.end_date IS NULL))
END
