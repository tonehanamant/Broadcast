-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCampaignsByIds]
	@ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		c.*
	FROM 
		campaigns c (NOLOCK) 
	WHERE 
		c.id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
