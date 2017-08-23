-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetAudienceItemsForProposal 32431
CREATE PROCEDURE [dbo].[usp_PCS_GetAudienceItemsForProposal]
	@proposal_id INT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	SET NOCOUNT ON;

    SELECT 
		pa.audience_id,
		a.name + CASE pa.ordinal WHEN 1 THEN ' (Primary)' ELSE ' (Demo ' + CAST(pa.ordinal AS VARCHAR(25)) + ')' END
	FROM 
		proposal_audiences pa (NOLOCK)
		JOIN audiences a (NOLOCK) ON a.id=pa.audience_id
	WHERE
		pa.proposal_id=@proposal_id
		AND pa.ordinal<>0
	ORDER BY 
		pa.ordinal
END
