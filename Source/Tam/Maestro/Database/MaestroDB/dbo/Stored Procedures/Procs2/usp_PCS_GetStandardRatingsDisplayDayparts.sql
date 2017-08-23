-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/29/2012
-- Description:	Get standard list of ratings dayparts.
-- =============================================
CREATE PROCEDURE usp_PCS_GetStandardRatingsDisplayDayparts
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		dp.id,
		dp.code,
		dp.name,
		dp.start_time,
		dp.end_time,
		dp.mon,
		dp.tue,
		dp.wed,
		dp.thu,
		dp.fri,
		dp.sat,
		dp.sun
	FROM
		dbo.daypart_maps dm (NOLOCK)
		JOIN dbo.vw_ccc_daypart dp ON dp.id=dm.daypart_id
	WHERE
		dm.map_set='FM_nPrpslList'
END
