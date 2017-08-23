-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- Changes:		1/19/2011	Modified so that only active zones are used when updating affidavits with missing zones.
-- =============================================
-- usp_ACS_AutoValidation_UpdateAffidavitZoneIds 340
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_UpdateAffidavitZoneIds]
	@media_month_id INT
AS
BEGIN
	UPDATE
		affidavits
	SET
		zone_id=(
			SELECT TOP 1
				zone_id
			FROM
				uvw_zone_universe z
			WHERE
				(z.start_date<=affidavits.air_date AND (z.end_date>=affidavits.air_date OR z.end_date IS NULL))
				AND	z.active=1
				AND z.code=affidavits.affidavit_syscode
		)
	FROM
		affidavits
	WHERE
		affidavits.media_month_id=@media_month_id
		AND affidavits.zone_id IS NULL
END
