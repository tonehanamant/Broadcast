-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographyZoneBusinessObjectsByTopography]
	@topography_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT zonements.
	SET NOCOUNT ON;

	SELECT
		topography_zones.topography_id,
		topography_zones.zone_id,
		topography_zones.include,
		topography_zones.effective_date,
		topographies.name,
		zones.code,
		zones.name
	FROM
		topography_zones
		JOIN topographies ON topographies.id=topography_zones.topography_id
		JOIN zones ON zones.id=topography_zones.zone_id
	WHERE
		topography_zones.topography_id=@topography_id
END
