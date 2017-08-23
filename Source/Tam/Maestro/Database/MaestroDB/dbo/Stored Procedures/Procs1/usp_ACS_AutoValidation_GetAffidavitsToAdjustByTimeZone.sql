-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/14/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_GetAffidavitsToAdjustByTimeZone]
	@media_month_id INT,
	@last_id INT
AS
BEGIN
	SET NOCOUNT ON;
	SET ROWCOUNT 100000;

    SELECT
		a.*
	FROM
		affidavits a (NOLOCK)
	WHERE
		a.media_month_id=@media_month_id
		AND a.zone_id		IS NOT NULL
		AND a.network_id	IS NOT NULL
		AND a.air_date		IS NOT NULL
		AND a.air_time		IS NOT NULL
		AND (a.adjusted_air_date IS NULL OR a.adjusted_air_time IS NULL)
		AND (@last_id IS NULL OR a.id > @last_id)
END
