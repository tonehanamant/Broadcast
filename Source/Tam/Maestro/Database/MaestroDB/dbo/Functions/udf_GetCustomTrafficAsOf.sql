CREATE FUNCTION [dbo].[udf_GetCustomTrafficAsOf] 
(     
      @dateAsOf as datetime
)
RETURNS TABLE
AS
RETURN
(
      SELECT DISTINCT
            sz.system_id,
            sz.zone_id,
            traffic_factor = 
				case  when ((traffic_order_format & 0x000003E0) / Power(2, 5)) = 4 then --bitshift right by 5 places, 4 = TrafficOrderFormatFlag.INS_ORD_TYPE_COPY_INSTRUCTIONS_PDF
					COALESCE(zct.traffic_factor, sct.traffic_factor, 1) -- if zone is not null, use its factor, else use system's factor, else use 1
				else
					ISNULL(sct.traffic_factor, 1)
				end,
            as_of_date = @dateAsOf
      FROM
			udf_GetSystemZonesAsOf(@dateAsOf) sz
			left join udf_GetZoneCustomTrafficAsOf(@dateAsOf) zct  on zct.zone_id = sz.zone_id
            left join udf_GetSystemCustomTrafficAsOf(@dateAsOf) sct on sct.system_id = sz.system_id
            left join udf_GetSystemsAsOf(@dateAsOf) s on sz.system_id = s.system_id

);

