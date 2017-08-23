-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: <10/6/2014>
-- Description:	<Description,,>
-- =============================================
--usp_PCS_DeleteMsaDeliveryFileById
CREATE PROCEDURE [dbo].[usp_PCS_DeleteMsaDeliveryFileById]
	@file_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @media_month_id INT;
	SELECT
		@media_month_id = mm.id
	FROM
		dbo.msa_delivery_files mdf (NOLOCK)
		JOIN dbo.media_months mm (NOLOCK) ON mm.start_date <= mdf.media_month_end_date AND mm.end_date >= mdf.media_month_start_date
	WHERE
		mdf.id=@file_id;
	
	DELETE FROM
		msa_deliveries
	WHERE
		media_month_id=@media_month_id
		AND msa_delivery_files_id=@file_id;

	DELETE FROM
		msa_delivery_files
	WHERE
		@file_id = msa_delivery_files.id
END
