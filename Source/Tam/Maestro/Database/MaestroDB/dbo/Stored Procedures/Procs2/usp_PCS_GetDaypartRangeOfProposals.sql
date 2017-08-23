-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/15/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDaypartRangeOfProposals]
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		CASE WHEN MIN(d.start_time) < MAX(d.start_time) THEN MIN(d.start_time) ELSE MAX(d.start_time) END 'start_time',
		CASE WHEN MIN(d.end_time)   < MAX(d.end_time)   THEN MIN(d.end_time)   ELSE MAX(d.end_time)   END 'end_time',
		MAX(d.mon) 'mon',
		MAX(d.tue) 'tue',
		MAX(d.wed) 'wed',
		MAX(d.thu) 'thu',
		MAX(d.fri) 'fri',
		MAX(d.sat) 'sat',
		MAX(d.sun) 'sun'
	FROM
		vw_ccc_daypart d
		JOIN proposal_details pd (NOLOCK) ON pd.daypart_id=d.id
			AND pd.proposal_id IN (
				SELECT id FROM dbo.SplitIntegers(@proposal_ids)
			)
END
