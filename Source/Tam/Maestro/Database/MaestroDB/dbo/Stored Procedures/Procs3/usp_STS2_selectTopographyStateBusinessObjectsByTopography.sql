-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographyStateBusinessObjectsByTopography]
	@topography_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		topography_states.topography_id,
		topography_states.state_id,
		topography_states.include,
		topography_states.effective_date,
		topographies.name,
		states.name
	FROM
		topography_states
		JOIN topographies ON topographies.id=topography_states.topography_id
		JOIN states ON states.id=topography_states.state_id
	WHERE
		topography_states.topography_id=@topography_id
END
