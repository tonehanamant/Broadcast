
create view [dbo].[custom_traffic] as 
with
z(
	zone_id,
	traffic_factor,
	effective_date
) as (
	select distinct
		zone_id,
		s.traffic_factor traffic_factor,
		s.effective_date effective_date
	from
		dbo.system_custom_traffic s with (nolock)
		join dbo.system_zones sz with (nolock) on
			s.system_id = sz.system_id
	where
		sz.type = 'TRAFFIC'
)	
select
	z.zone_id zone_id,
	n.id network_id,
	z.traffic_factor traffic_factor,
	z.effective_date effective_date
from
	z
	cross join networks n
where
	n.active = 1;
