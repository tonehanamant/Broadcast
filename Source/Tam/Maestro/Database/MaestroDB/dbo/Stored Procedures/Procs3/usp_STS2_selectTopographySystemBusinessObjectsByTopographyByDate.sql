-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographySystemBusinessObjectsByTopographyByDate]
	@topography_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT systemments.
	SET NOCOUNT ON;

	SELECT
		uvw_topography_system_universe.topography_id,
		uvw_topography_system_universe.system_id,
		uvw_topography_system_universe.start_date,
		uvw_topography_system_universe.include,
		uvw_topography_system_universe.end_date,
		topographies.name,
		uvw_system_universe.code
	FROM
		uvw_topography_system_universe
		JOIN topographies ON topographies.id=uvw_topography_system_universe.topography_id
		JOIN uvw_system_universe ON uvw_system_universe.system_id=uvw_topography_system_universe.system_id	AND (uvw_system_universe.start_date<=@effective_date AND (uvw_system_universe.end_date>=@effective_date OR uvw_system_universe.end_date IS NULL))
	WHERE
		uvw_topography_system_universe.topography_id=@topography_id
		AND (uvw_topography_system_universe.start_date<=@effective_date AND (uvw_topography_system_universe.end_date>=@effective_date OR uvw_topography_system_universe.end_date IS NULL))
END
