-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographySystemGroupBusinessObjectsByTopography]
	@topography_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT system_groupments.
	SET NOCOUNT ON;

	SELECT
		topography_system_groups.topography_id,
		topography_system_groups.system_group_id,
		topography_system_groups.include,
		topography_system_groups.effective_date,
		topographies.name,
		system_groups.name
	FROM
		topography_system_groups
		JOIN topographies ON topographies.id=topography_system_groups.topography_id
		JOIN system_groups ON system_groups.id=topography_system_groups.system_group_id
	WHERE
		topography_system_groups.topography_id=@topography_id
END
