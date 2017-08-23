CREATE FUNCTION [dbo].[udf_GetZoneCustomTrafficAsOf]
(     
      @dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN
(
      select
            [zone_id], 
            [traffic_factor], 
            @dateAsOf as [as_of_date]
      from
            zone_custom_traffic zct WITH (NOLOCK)
      where
            @dateAsOf >= zct.effective_date

      union

      select
            [zone_id], 
            [traffic_factor], 
            @dateAsOf as [as_of_date]
      from
            zone_custom_traffic_histories zcth WITH (NOLOCK)
      where
            @dateAsOf between zcth.start_date and zcth.end_date
);
