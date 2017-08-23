CREATE PROCEDURE [dbo].[usp_dma_audiences_select_all]
AS
SELECT
	*
FROM
	dbo.dma_audiences WITH(NOLOCK)
