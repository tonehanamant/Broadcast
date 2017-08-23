


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayDmasByDate]
	@active bit,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	(
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
			(@active IS NULL OR active=@active)
			AND id NOT IN (
				SELECT dma_id FROM uvw_dma_universe (NOLOCK) WHERE 
					(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
			)
	)
	ORDER BY 
		name
END



