
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectMaximumTopographyDmaDate]
	@topography_id int,
	@dma_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT MAX(start_date) FROM uvw_topography_dma_universe (NOLOCK) WHERE topography_id=@topography_id AND dma_id=@dma_id
END

