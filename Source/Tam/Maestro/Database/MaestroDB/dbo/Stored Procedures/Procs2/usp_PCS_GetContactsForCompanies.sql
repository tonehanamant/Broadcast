

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetContactsForCompanies]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		c.* 
	FROM 
		contacts c (NOLOCK) 
	WHERE 
		c.company_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
