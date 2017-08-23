-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectSystemDaypartBusinessObjectsBySystem]
	@system_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT	
		system_dayparts.system_id,
		system_dayparts.daypart_id,
		system_dayparts.effective_date,
		vw_ccc_daypart.id,
		vw_ccc_daypart.code,
		vw_ccc_daypart.name,
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
		system_dayparts
		JOIN vw_ccc_daypart ON vw_ccc_daypart.id=system_dayparts.daypart_id
	WHERE 
		system_id=@system_id
	ORDER BY name
END
