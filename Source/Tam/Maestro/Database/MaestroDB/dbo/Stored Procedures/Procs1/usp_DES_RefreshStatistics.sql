-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/13/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_DES_RefreshStatistics]
AS
BEGIN
	UPDATE STATISTICS dbo.affidavits;
	UPDATE STATISTICS dbo.affidavit_deliveries;
	UPDATE STATISTICS dbo.tam_post_affidavits;
END
