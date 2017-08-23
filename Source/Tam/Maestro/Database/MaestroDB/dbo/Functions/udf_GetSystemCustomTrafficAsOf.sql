CREATE FUNCTION [dbo].[udf_GetSystemCustomTrafficAsOf]
(     
      @dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN
(
      select
            [system_id], 
            [traffic_factor], 
            @dateAsOf as [as_of_date]
      from
            system_custom_traffic (nolock)
      where
            @dateAsOf >= system_custom_traffic.effective_date

      union

      select
            [system_id], 
            [traffic_factor], 
            @dateAsOf as [as_of_date]
      from
            system_custom_traffic_histories (nolock)
      where
            @dateAsOf between system_custom_traffic_histories.start_date and system_custom_traffic_histories.end_date
);
