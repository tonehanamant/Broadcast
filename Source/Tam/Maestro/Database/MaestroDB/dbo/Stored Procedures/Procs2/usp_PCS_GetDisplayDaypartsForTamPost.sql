-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/15/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayDaypartsForTamPost]
	@tam_post_id INT
AS
BEGIN
	SELECT DISTINCT
		d.id,
		d.code,
		d.name,
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
		JOIN tam_post_dayparts tpd (NOLOCK) ON tpd.daypart_id=d.id
			AND tpd.tam_post_id=@tam_post_id
END
