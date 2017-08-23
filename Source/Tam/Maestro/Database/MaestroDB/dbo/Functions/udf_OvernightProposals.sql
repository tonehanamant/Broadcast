-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/30/2011
-- Description: Returns a list of proposal id's that are overnight.
-- =============================================
CREATE FUNCTION udf_OvernightProposals()
RETURNS TABLE 
AS
RETURN 
(
	WITH overnight_dayparts(
		daypart_id
	) AS (
		SELECT
			dp.id daypart_id
		FROM
			dayparts dp WITH(NOLOCK)
			join timespans ts WITH(NOLOCK) ON
				ts.id = dp.timespan_id
		WHERE
			(ts.start_time BETWEEN 72000 AND 86400 OR ts.start_time BETWEEN 0 AND 21599)
			AND
			ts.end_time BETWEEN 0 AND 21599
	)
	
	SELECT DISTINCT
		p.id proposal_id
	FROM
		proposals p WITH(NOLOCK)
		JOIN proposal_details pd WITH(NOLOCK) ON
			p.id = pd.proposal_id
		JOIN overnight_dayparts o_dp ON
			o_dp.daypart_id = pd.daypart_id
)
