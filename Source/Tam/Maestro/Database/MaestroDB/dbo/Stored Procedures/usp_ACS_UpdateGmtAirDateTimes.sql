-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/23/2017
-- Modified:	4/17/2017 - Fixed incorrect gmt_air_datetime calculation.
-- Description:	Updates the GMT Air DateTime for affadavits by media month
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_UpdateGmtAirDateTimes]
	@media_month_id INT
AS
BEGIN
	UPDATE
		affidavits
	SET
		gmt_air_datetime =
			DATEADD(
				SECOND,
				a.air_time - tz_local.utc_std_offset_seconds,
				a.air_date
			)
	FROM
		affidavits a
		JOIN dbo.uvw_zone_universe z ON z.zone_id=a.zone_id
			AND z.start_date<=a.air_date AND (z.end_date>=a.air_date OR z.end_date IS NULL)
		JOIN tri.time_zones tz_local ON tz_local.tz_time_zone_name=
			CASE z.time_zone_id 
				WHEN 1 THEN 'Eastern D.S.' 
				WHEN 2 THEN 'Central D.S.' 
				WHEN 3 THEN 'Mountain D.S.' 
				WHEN 4 THEN 'Pacific D.S.' 
				WHEN 5 THEN 'Alaskan D.S.' 
				WHEN 6 THEN 'Hawaiian'
			END
			AND (tz_local.start_date IS NULL OR DATEADD(second, a.air_time, a.air_date) BETWEEN tz_local.start_date AND tz_local.end_date)
	WHERE
		a.media_month_id=@media_month_id
		AND a.zone_id IS NOT NULL 
		AND a.network_id IS NOT NULL
		AND a.gmt_air_datetime IS NULL;
END