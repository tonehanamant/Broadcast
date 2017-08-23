-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectSystemGroupSystemBusinessObjectsBySystemGroupByDate]
	@system_group_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT	uvw_systemgroupsystem_universe.system_group_id,
			uvw_systemgroupsystem_universe.system_id,
			uvw_systemgroupsystem_universe.start_date,
			uvw_systemgroupsystem_universe.end_date,
			uvw_system_universe.code
	FROM uvw_systemgroupsystem_universe
	JOIN uvw_system_universe ON uvw_system_universe.system_id=uvw_systemgroupsystem_universe.system_id AND (uvw_system_universe.start_date<=@effective_date AND (uvw_system_universe.end_date>=@effective_date OR uvw_system_universe.end_date IS NULL))
	WHERE 
		uvw_systemgroupsystem_universe.system_group_id=@system_group_id
		AND (uvw_systemgroupsystem_universe.start_date<=@effective_date AND (uvw_systemgroupsystem_universe.end_date>=@effective_date OR uvw_systemgroupsystem_universe.end_date IS NULL))
	ORDER BY 
		uvw_system_universe.code
END
