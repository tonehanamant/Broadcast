-- =============================================
-- Author:		MNorris
-- Create date: 04/21/16
-- Description:	Gets zone zips for zone_id
-- =============================================
-- usp_STS2_SelectZoneZipCodesByZoneId 120
CREATE PROCEDURE dbo.usp_STS2_SelectZoneZipCodesByZoneId
	@zone_id int
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		zzc.*
	FROM
		zone_zip_codes zzc
	WHERE
		zzc.zone_id=@zone_id
	ORDER BY
		zzc.zip_code;
END