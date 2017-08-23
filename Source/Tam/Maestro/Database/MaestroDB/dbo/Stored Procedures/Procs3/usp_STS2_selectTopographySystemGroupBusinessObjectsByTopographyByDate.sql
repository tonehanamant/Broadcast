-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographySystemGroupBusinessObjectsByTopographyByDate]
	@topography_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT system_groupments.
	SET NOCOUNT ON;

	SELECT
		uvw_topography_system_group_universe.topography_id,
		uvw_topography_system_group_universe.system_group_id,
		uvw_topography_system_group_universe.start_date,
		uvw_topography_system_group_universe.include,
		uvw_topography_system_group_universe.end_date,
		topographies.name,
		uvw_systemgroup_universe.name
	FROM
		uvw_topography_system_group_universe
		JOIN topographies ON topographies.id=uvw_topography_system_group_universe.topography_id
		JOIN uvw_systemgroup_universe ON uvw_systemgroup_universe.system_group_id=uvw_topography_system_group_universe.system_group_id	AND (uvw_systemgroup_universe.start_date<=@effective_date AND (uvw_systemgroup_universe.end_date>=@effective_date OR uvw_systemgroup_universe.end_date IS NULL))
	WHERE
		uvw_topography_system_group_universe.topography_id=@topography_id
		AND (uvw_topography_system_group_universe.start_date<=@effective_date AND (uvw_topography_system_group_universe.end_date>=@effective_date OR uvw_topography_system_group_universe.end_date IS NULL))
END
