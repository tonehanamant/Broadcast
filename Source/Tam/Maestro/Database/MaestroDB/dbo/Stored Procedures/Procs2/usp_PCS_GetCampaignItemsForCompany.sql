
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetCampaignItemsForCompany
	@company_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		name 
	FROM 
		campaigns (NOLOCK)
	WHERE 
		company_id=@company_id 
	ORDER BY 
		date_created
END

