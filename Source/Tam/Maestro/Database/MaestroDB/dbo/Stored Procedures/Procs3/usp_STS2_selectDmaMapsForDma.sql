-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDmaMapsForDma]
	@dma_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		id,
		dma_id,
		map_set,
		map_value,
		active,
		flag,
		effective_date
	FROM
		dma_maps
	WHERE
		dma_id=@dma_id
END
