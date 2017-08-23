-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographyBusinessBusinessObjectsByTopography]
	@topography_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		topography_businesses.topography_id,
		topography_businesses.business_id,
		topography_businesses.include,
		topography_businesses.effective_date,
		topographies.name,
		businesses.name
	FROM
		topography_businesses
		JOIN topographies ON topographies.id=topography_businesses.topography_id
		JOIN businesses ON businesses.id=topography_businesses.business_id
	WHERE
		topography_businesses.topography_id=@topography_id
END
