


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayDmaByDate]
	@dma_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT	
		dma_id,
		code,
		name,
		rank,
		tv_hh,
		cable_hh,
		active,
		start_date,
		flag,
		end_date,
		dbo.GetSubscribersForDma(dma_id,@effective_date,1,null)
	FROM 
		uvw_dma_universe (NOLOCK) 
	WHERE 
		(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
		AND dma_id=@dma_id

	UNION

	SELECT	
		id,
		code,
		name,
		rank,
		tv_hh,
		cable_hh,
		active,
		effective_date,
		flag,
		null,
		dbo.GetSubscribersForDma(id,@effective_date,1,null)
	FROM 
		dmas (NOLOCK) 
	WHERE 
		id=@dma_id
		AND id NOT IN (
			SELECT dma_id FROM uvw_dma_universe (NOLOCK) WHERE 
				(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
				AND dma_id=@dma_id
		)
END



