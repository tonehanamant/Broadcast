-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographyDmaBusinessObjectsByTopography]
	@topography_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		topography_dmas.topography_id,
		topography_dmas.dma_id,
		topography_dmas.include,
		topography_dmas.effective_date,
		topographies.name,
		dmas.name
	FROM
		topography_dmas
		JOIN topographies ON topographies.id=topography_dmas.topography_id
		JOIN dmas ON dmas.id=topography_dmas.dma_id
	WHERE
		topography_dmas.topography_id=@topography_id
END
