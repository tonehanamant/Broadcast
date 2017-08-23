


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_REL_GetSystemTrafficFormat]
	@system_id as int
AS

with
non_traffic_sbts (
	system_id
) as (
	select
		sz.system_id
	from
		systems s (NOLOCK)
		join system_zones sz (NOLOCK) on
			s.id = sz.system_id
		join zones z (NOLOCK) on
			z.id = sz.zone_id
		join zone_networks (NOLOCK) zn on
			z.id = zn.zone_id
	where
		s.active = 1
		and
		sz.type = 'TRAFFIC'
		and
		s.traffic_order_format & 0x00000400 <> 0x00000400
		and 
		s.traffic_order_format & 0x00000800 <> 0x00000800
		and
		z.traffic = 1
		and
		zn.trafficable = 1
	group by
		sz.system_id
)	
select
	s.code, s.name, s.location, s.spot_yield_weight, 
	case s.traffic_order_format & 0x000003E0
		when 0x00000020 then 'Standard'
		when 0x00000040 then 'Spot-Rate'
		when 0x00000060 then 'DP Spot-Rate'
		else 'Unknown'
	end system_order_type,
	case s.traffic_order_format & 0x00000800
		when 0x00000800 then 1
		else 0
	end system_xml_order,
	case s.traffic_order_format & 0x00000400
		when 0x00000400 then 1
		else 0
	end system_pdf_order,
	case 
		when s.traffic_order_format & 0x00001000 = 0x00001000 then 1
		when s.traffic_order_format & 0x00001000 <> 0x00001000 then 0
		else -1
	end system_confirmation,
	case 
		when s.traffic_order_format & 0x00002000 = 0x00002000 then 1
		when s.traffic_order_format & 0x00002000 <> 0x00002000 then 0
		else -1
	end system_lineup,
	case 
		when s.traffic_order_format & 0x00004000 = 0x00004000 then 1
		when s.traffic_order_format & 0x00004000 <> 0x00004000 then 0
		else -1
	end system_alerts,
	case 
		when s.traffic_order_format & 0x00008000 = 0x00008000 then 1
		when s.traffic_order_format & 0x00008000 <> 0x00008000 then 0
		else -1
	end system_orders,
	case 
		when s.traffic_order_format & 0x00010000 = 0x00010000 then 1
		when s.traffic_order_format & 0x00010000 <> 0x00010000 then 0
		else -1
	end system_cutsheet,
	case s.traffic_order_format & 0x00020000
		when 0x00020000 then 1
		else 0
	end system_multimarket,
	case s.traffic_order_format & 0x00040000
		when 0x00040000 then 1
		else 0
	end system_store_spot_yield,
	case s.traffic_order_format & 0x00080000
		when 0x00080000 then 1
		else 0
	end system_fixed_rate
from
	systems s (NOLOCK)
where s.id = @system_id
order by
	s.code;


