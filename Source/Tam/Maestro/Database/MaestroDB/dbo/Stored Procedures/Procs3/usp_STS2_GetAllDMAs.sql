CREATE Procedure [dbo].[usp_STS2_GetAllDMAs]
AS

DECLARE @effective_date DATETIME
SET @effective_date = getdate()

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
      dbo.GetSubscribersForDma(dma_id,@effective_date,1,null) 'subscribers'
FROM 
      uvw_dma_universe (NOLOCK)
WHERE 
      (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
      AND code > 0 AND dma_id IN (
            SELECT dma_id FROM uvw_zonedma_universe WHERE 
                  (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
      )
ORDER BY
      rank ASC

