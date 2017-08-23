
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetMMGDayparts]
AS
BEGIN
    SELECT 
		vw_ccc_daypart.id,
		vw_ccc_daypart.code,
		daypart_maps.map_value,
		vw_ccc_daypart.start_time,
		vw_ccc_daypart.end_time,
		vw_ccc_daypart.mon,
		vw_ccc_daypart.tue,
		vw_ccc_daypart.wed,
		vw_ccc_daypart.thu,
		vw_ccc_daypart.fri,
		vw_ccc_daypart.sat,
		vw_ccc_daypart.sun
	FROM 
		daypart_maps (NOLOCK)
		JOIN vw_ccc_daypart ON vw_ccc_daypart.id=daypart_maps.daypart_id
	WHERE 
		map_set='MMG'
END

