-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/23/2012
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetAudiencesByProposal 34116
CREATE PROCEDURE [dbo].[usp_PCS_GetAudiencesByProposal]
	@proposal_id INT
AS
BEGIN
	SELECT
		a.*
	FROM
		audiences a (NOLOCK)
		JOIN proposal_audiences pa (NOLOCK) ON pa.audience_id=a.id
			AND pa.proposal_id=@proposal_id
	ORDER BY
		pa.ordinal
END
