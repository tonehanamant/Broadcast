-- =============================================
-- Author:        Nicholas Kheynis
-- Create date:	  8/15/2014
-- Description:   <Description,,>
-- =============================================
-- usp_ACS_LookupProposalRateByWeekPerProposal 21007
CREATE PROCEDURE [dbo].[usp_ACS_LookupProposalRateByWeekPerProposal]
      @proposal_id INT
AS
BEGIN
	
	SELECT DISTINCT
		SUM(pd.proposal_rate),
		mw.START_DATE,
		mw.end_date
	FROM
		dbo.proposal_details pd
		JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id = pd.id
		JOIN dbo.media_weeks mw (NOLOCK) ON mw.id = pdw.media_week_id
		JOIN dbo.proposals p (NOLOCK) ON p.id = pd.proposal_id
	WHERE
		pd.proposal_id = @proposal_id
	GROUP BY
		mw.START_DATE,
		mw.end_date

	

END

