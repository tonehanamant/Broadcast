-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/6/2012
-- Update date: 3/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_DES_GetDeliveryLastRunDate]
	@media_month_id INT
AS
BEGIN
	SELECT 
		MAX(adr.date_completed) 
	FROM 
		dbo.affidavit_delivery_runs adr (NOLOCK)
	WHERE
		adr.media_month_id=@media_month_id
		AND adr.status_code=2
END
