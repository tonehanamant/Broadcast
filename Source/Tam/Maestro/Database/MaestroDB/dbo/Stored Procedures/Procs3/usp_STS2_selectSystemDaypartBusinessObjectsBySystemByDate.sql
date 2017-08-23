-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectSystemDaypartBusinessObjectsBySystemByDate]
	@system_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT	
		uvw_systemdaypart_universe.system_id,
		uvw_systemdaypart_universe.daypart_id,
		uvw_systemdaypart_universe.start_date,
		uvw_systemdaypart_universe.end_date,
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
		uvw_systemdaypart_universe
		JOIN vw_ccc_daypart ON vw_ccc_daypart.id=uvw_systemdaypart_universe.daypart_id
	WHERE 
		uvw_systemdaypart_universe.system_id=@system_id
		AND (uvw_systemdaypart_universe.start_date<=@effective_date AND (uvw_systemdaypart_universe.end_date>=@effective_date OR uvw_systemdaypart_universe.end_date IS NULL))
	ORDER BY 
		name
END
