-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographyZoneBusinessObjectsByTopographyByDate]
	@topography_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT zonements.
	SET NOCOUNT ON;

	SELECT
		uvw_topography_zone_universe.topography_id,
		uvw_topography_zone_universe.zone_id,
		uvw_topography_zone_universe.start_date,
		uvw_topography_zone_universe.include,
		uvw_topography_zone_universe.end_date,
		topographies.name,
		uvw_zone_universe.code,
		uvw_zone_universe.name
	FROM
		uvw_topography_zone_universe
		JOIN topographies ON topographies.id=uvw_topography_zone_universe.topography_id
		JOIN uvw_zone_universe ON uvw_zone_universe.zone_id=uvw_topography_zone_universe.zone_id	AND (uvw_zone_universe.start_date<=@effective_date AND (uvw_zone_universe.end_date>=@effective_date OR uvw_zone_universe.end_date IS NULL))
	WHERE
		uvw_topography_zone_universe.topography_id=@topography_id
		AND (uvw_topography_zone_universe.start_date<=@effective_date AND (uvw_topography_zone_universe.end_date>=@effective_date OR uvw_topography_zone_universe.end_date IS NULL))
END
