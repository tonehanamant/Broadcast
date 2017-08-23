-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: <10/6/2014>
-- Description:	<Description,,>
-- =============================================
--usp_PCS_GetMSADeliveryFilesByMediaMonthId 394
CREATE PROCEDURE [dbo].[usp_PCS_GetMSADeliveryFilesByMediaMonthId]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		mdf.*,
		d.file_name,
		e.firstname,
		e.lastname
	FROM
		dbo.msa_delivery_files mdf (NOLOCK)
		JOIN documents d (NOLOCK)ON d.id = mdf.document_id
		JOIN dbo.employees e (NOLOCK) ON e.id = mdf.employee_id
		JOIN dbo.media_months mm (NOLOCK) ON mm.start_date <= mdf.media_month_end_date AND mm.end_date >= mdf.media_month_start_date
			AND mm.id=@media_month_id
END
