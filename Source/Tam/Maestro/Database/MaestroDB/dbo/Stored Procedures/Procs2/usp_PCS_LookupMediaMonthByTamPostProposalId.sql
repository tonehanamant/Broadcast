-- =============================================
-- Author:        Nicholas Kheynis
-- Create date: 8/4/2014
-- Description:   <Description,,>
-- =============================================
-- usp_PCS_LookupMediaMonthByTamPostProposalId 2
CREATE PROCEDURE [dbo].[usp_PCS_LookupMediaMonthByTamPostProposalId]
	  @tam_post_proposal_id INT
AS
BEGIN
	CREATE TABLE #tmp (start_date DATETIME, end_date DATETIME)
	INSERT INTO #tmp
		SELECT 
			p.start_date,
			p.end_date 
		FROM 
			dbo.tam_post_proposals tpp
			JOIN dbo.proposals p(NOLOCK) ON p.id = tpp.posting_plan_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id
		
	SELECT
		mm.*
	FROM
		dbo.media_months mm (NOLOCK)
		INNER JOIN #tmp t (NOLOCK) ON t.start_date >= mm.start_date
		AND t.end_date <= mm.end_date

	DROP TABLE #tmp;		
END
