-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/25/2011
-- Description:	
-- =============================================
-- usp_ACS_StpDeleteOutOfSpecAffidavitTrafficRecords 356
CREATE PROCEDURE [dbo].[usp_ACS_StpDeleteOutOfSpecAffidavitTrafficRecords]
	@media_month_id INT
AS
BEGIN
	DELETE FROM 
		affidavit_traffic
	WHERE
		media_month_id=@media_month_id
		AND status_code IN (0,2)
END
