

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalsForProducts]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		*
	FROM 
		proposals (NOLOCK)
	WHERE 
		product_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		) 
END

