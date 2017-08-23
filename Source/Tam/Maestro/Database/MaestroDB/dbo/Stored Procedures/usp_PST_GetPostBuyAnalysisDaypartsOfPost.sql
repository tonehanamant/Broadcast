-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/3/2016
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PST_GetPostBuyAnalysisDaypartsOfPost]
	@tam_post_id INT
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
	WHERE
		d.id IN (
			SELECT tpd.daypart_id FROM tam_post_dayparts tpd (NOLOCK) WHERE tpd.tam_post_id=@tam_post_id
		)
END