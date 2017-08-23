-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: <10/6/2014>
-- Description:	<Description,,>
-- =============================================
--usp_PCS_GetMediaMonthsForMsaDeliveryFiles
CREATE PROCEDURE [dbo].[usp_PCS_GetMediaMonthsForMsaDeliveryFiles]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		mm.*
	FROM
		dbo.msa_delivery_files mdf (NOLOCK)
		JOIN dbo.media_months mm (NOLOCK) ON mm.start_date <= mdf.media_month_end_date AND mm.end_date >= mdf.media_month_start_date		
END
