
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_FOG_GetProposalDetails]
	@proposal_id INT,
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		pd.network_id,
		SUM(pdw.units) 'num_spots',
		pd.topography_universe,
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
		proposal_details pd (NOLOCK)
		JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
		JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
			AND mw.media_month_id=@media_month_id
		JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
	WHERE
		pd.proposal_id=@proposal_id
		AND pd.num_spots > 0
	GROUP BY
		pd.network_id,
		pd.topography_universe,
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
	ORDER BY
		pd.network_id	
END

