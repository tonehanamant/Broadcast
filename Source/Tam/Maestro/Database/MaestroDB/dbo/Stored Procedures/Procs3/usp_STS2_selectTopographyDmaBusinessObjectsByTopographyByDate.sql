-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectTopographyDmaBusinessObjectsByTopographyByDate]
	@topography_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		uvw_topography_dma_universe.topography_id,
		uvw_topography_dma_universe.dma_id,
		uvw_topography_dma_universe.start_date,
		uvw_topography_dma_universe.include,
		uvw_topography_dma_universe.end_date,
		topographies.name,
		uvw_dma_universe.name
	FROM
		uvw_topography_dma_universe
		JOIN topographies ON topographies.id=uvw_topography_dma_universe.topography_id
		JOIN uvw_dma_universe ON uvw_dma_universe.dma_id=uvw_topography_dma_universe.dma_id	AND (uvw_dma_universe.start_date<=@effective_date AND (uvw_dma_universe.end_date>=@effective_date OR uvw_dma_universe.end_date IS NULL))
	WHERE
		uvw_topography_dma_universe.topography_id=@topography_id
		AND (uvw_topography_dma_universe.start_date<=@effective_date AND (uvw_topography_dma_universe.end_date>=@effective_date OR uvw_topography_dma_universe.end_date IS NULL))
END
