-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCampaignsByCompanyIds]
	@ids VARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		c.*
	FROM 
		campaigns c (NOLOCK) 
	WHERE 
		c.company_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
		OR c.agency_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END

