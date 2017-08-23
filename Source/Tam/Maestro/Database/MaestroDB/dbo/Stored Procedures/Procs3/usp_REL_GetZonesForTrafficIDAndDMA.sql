/* BEGIN CRMWRC-578 performance enhancements for the SCX generation */
-- Updated to take in an effective date so that the calculation for it will be done only one.
-- The effective date calculation was taking up half of the query time.
CREATE PROCEDURE [dbo].[usp_REL_GetZonesForTrafficIDAndDMA]
      @system_id int,
      @traffic_id int,
      @dma_id int,
      @effective_date datetime
AS

select 
      distinct traffic_orders.zone_id
from 
      traffic_orders with (NOLOCK)
where traffic_orders.traffic_id = @traffic_id 
      and traffic_orders.system_id = @system_id 
      and traffic_orders.active = 1
      and traffic_orders.zone_id 
      in
      (
            select zone_id from uvw_zonedma_universe where dma_id = @dma_id and 
            (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
      )
