


CREATE Procedure [dbo].[usp_STS2_GetDMAsForSystem]
      (
            @system_id Int,
            @traffic_id int
      )
AS

DECLARE @effective_date DATETIME

select @effective_date = min(traffic_orders.start_date) 
from traffic_orders (NOLOCK) join traffic_details (NOLOCK) 
on traffic_orders.traffic_detail_id = traffic_details.id 
where traffic_details.traffic_id = @traffic_id and traffic_orders.ordered_spots > 0 and traffic_orders.system_id = @system_id;

if(@effective_date is NULL)
BEGIN
select @effective_date = traffic.start_date 
from traffic (NOLOCK)
where traffic.id = @traffic_id;
END

SELECT      
      uvw_dma_universe.dma_id,
      uvw_dma_universe.code,
      isnull(dma_maps.map_value, uvw_dma_universe.name),
      uvw_dma_universe.rank,
      uvw_dma_universe.tv_hh,
      uvw_dma_universe.cable_hh,
      uvw_dma_universe.active,
      uvw_dma_universe.start_date,
      uvw_dma_universe.flag,
      uvw_dma_universe.end_date,
      dbo.GetSubscribersForDma(uvw_dma_universe.dma_id,@effective_date,1,1) 'subscribers'
FROM 
      uvw_dma_universe (NOLOCK) 
            left join dma_maps (NOLOCK) on dma_maps.dma_id = uvw_dma_universe.dma_id and dma_maps.map_set = 'Strata'
WHERE 
        (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
      AND uvw_dma_universe.dma_id IN (
            SELECT dma_id FROM uvw_zonedma_universe WHERE 
                  (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
                              AND zone_id IN (
                        select distinct zone_id from uvw_systemzone_universe sz
                                    where sz.system_id = @system_id and sz.type='traffic' and (sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL))
            )
      )
ORDER BY
      rank ASC
