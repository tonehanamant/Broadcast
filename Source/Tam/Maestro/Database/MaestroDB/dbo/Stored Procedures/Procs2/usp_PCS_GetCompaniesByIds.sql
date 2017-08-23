-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description: 
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCompaniesByIds]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		c.*
	FROM 
		companies c (NOLOCK) 
	WHERE 
		c.id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
