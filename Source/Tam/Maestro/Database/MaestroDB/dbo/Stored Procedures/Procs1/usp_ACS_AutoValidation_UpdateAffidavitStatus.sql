-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_UpdateAffidavitStatus]
	@media_month_id INT
AS
BEGIN
	UPDATE
		affidavits
	SET
		status_id=1
	FROM
		affidavits
	WHERE
		media_month_id=@media_month_id
		AND status_id=2
		AND material_id		IS NOT NULL
		AND zone_id			IS NOT NULL
		AND network_id		IS NOT NULL
		AND spot_length_id	IS NOT NULL
		AND air_date		IS NOT NULL
		AND air_time		IS NOT NULL
		AND rate			IS NOT NULL
		AND subscribers		IS NOT NULL
		AND subscribers > 0
		--AND traffic_id		IS NOT NULL
END
