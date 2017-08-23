-- =============================================
-- Author:		MNorris
-- Create date: 04/21/16
-- Description:	Gets zone zips for zone_id
-- =============================================
-- usp_STS2_SelectZoneZipCodesByZoneId 120
CREATE PROCEDURE dbo.usp_STS2_SelectZoneZipCodesByZoneIdAndDate
	@zone_id INT,
	@effective_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		zzc.*
	FROM
		uvw_zone_zip_code_universe zzc
	WHERE
		zzc.zone_id=@zone_id
		AND zzc.start_date<=@effective_date AND (zzc.end_date>=@effective_date OR zzc.end_date IS NULL)
	ORDER BY
		zzc.zip_code;
END