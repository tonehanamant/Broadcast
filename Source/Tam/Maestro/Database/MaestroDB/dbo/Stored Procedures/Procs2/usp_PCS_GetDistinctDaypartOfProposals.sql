-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/25/2013
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDistinctDaypartOfProposals]
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
	SELECT DISTINCT
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun
	FROM
		vw_ccc_daypart d
		JOIN proposal_details pd (NOLOCK) ON pd.daypart_id=d.id
			AND pd.proposal_id IN (
				SELECT id FROM dbo.SplitIntegers(@proposal_ids)
			)
END
