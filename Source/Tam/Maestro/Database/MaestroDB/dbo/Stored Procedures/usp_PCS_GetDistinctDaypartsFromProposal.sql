-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: 4/13/2015
-- Description:	Get distinct dayparts from proposal details per proposal.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDistinctDaypartsFromProposal]
	@proposal_id INT
AS
BEGIN
	SELECT DISTINCT
	  d.daypart_text
FROM
	  proposal_details pd (NOLOCK)
	  JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
WHERE
	  pd.proposal_id=@proposal_id
	  AND pd.num_spots>0
END