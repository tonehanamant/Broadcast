-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographySystemsByTopography]
	@topography_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		topography_id,
		system_id,
		include,
		effective_date
	FROM
		topography_systems
	WHERE
		topography_id=@topography_id
END
