-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographySystemBusinessObjectsByTopography]
	@topography_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT systemments.
	SET NOCOUNT ON;

	SELECT
		topography_systems.topography_id,
		topography_systems.system_id,
		topography_systems.include,
		topography_systems.effective_date,
		topographies.name,
		systems.code
	FROM
		topography_systems
		JOIN topographies ON topographies.id=topography_systems.topography_id
		JOIN systems ON systems.id=topography_systems.system_id
	WHERE
		topography_systems.topography_id=@topography_id
END
