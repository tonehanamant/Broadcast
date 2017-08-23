
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/26/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetBroadcastProposalsForProducts]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		bp.*
	FROM 
		broadcast_proposals bp (NOLOCK)
	WHERE 
		bp.product_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END