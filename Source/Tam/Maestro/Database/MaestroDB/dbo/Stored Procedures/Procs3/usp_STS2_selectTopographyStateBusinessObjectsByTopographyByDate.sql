-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographyStateBusinessObjectsByTopographyByDate]
	@topography_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		uvw_topography_state_universe.topography_id,
		uvw_topography_state_universe.state_id,
		uvw_topography_state_universe.start_date,
		uvw_topography_state_universe.include,
		uvw_topography_state_universe.end_date,
		topographies.name,
		uvw_state_universe.name
	FROM
		uvw_topography_state_universe
		JOIN topographies ON topographies.id=uvw_topography_state_universe.topography_id
		JOIN uvw_state_universe ON uvw_state_universe.state_id=uvw_topography_state_universe.state_id	AND (uvw_state_universe.start_date<=@effective_date AND (uvw_state_universe.end_date>=@effective_date OR uvw_state_universe.end_date IS NULL))
	WHERE
		uvw_topography_state_universe.topography_id=@topography_id
		AND (uvw_topography_state_universe.start_date<=@effective_date AND (uvw_topography_state_universe.end_date>=@effective_date OR uvw_topography_state_universe.end_date IS NULL))
END
