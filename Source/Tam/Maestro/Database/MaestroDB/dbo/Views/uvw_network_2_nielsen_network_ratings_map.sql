create view [dbo].[uvw_network_2_nielsen_network_ratings_map] as 
	with
	all_nielsen_maps(
		network_id, 
		nielsen_id, 
		start_date, 
		end_date
	) as (
		select
			network_id,
			cast(map_value as int) nielsen_id,
			effective_date start_date,
			'12/31/2032' end_date
		from
			network_maps
		where
			map_set = 'Nielsen'
		union all
		select
			network_id,
			cast(map_value as int) nielsen_id,
			start_date,
			end_date
		from
			network_map_histories
		where
			map_set = 'Nielsen'
	),
	all_nielsen_networks(
		nielsen_network_id,
		nielsen_id,
		start_date,
		end_date
	) as (
		select
			id nielsen_network_id,
			nielsen_id,
			effective_date start_date,
			'12/31/2032' end_date
		from
			nielsen_networks
		union
		select
			nielsen_network_id,
			nielsen_id,
			start_date,
			end_date
		from
			nielsen_network_histories
	),
	all_daypart_maps(
		national_network_id, 
		daypart_network_id, 
		start_date, 
		end_date
	) as (
		select
			network_map.network_id national_network_id,
			daypart_network.id daypart_network_id,
			network_map.effective_date start_date,
			'12/31/2032' end_date
		from
			network_maps network_map
			join networks daypart_network on
				daypart_network.id = network_map.map_value
		where
			map_set = 'DaypartNetworks'
			and
			network_map.network_id <> daypart_network.id
		union all
		select
			network_map.network_id national_network_id,
			daypart_network.id daypart_network_id,
			network_map.start_date,
			network_map.end_date
		from
			network_map_histories network_map
			join networks daypart_network on
				daypart_network.code = network_map.map_value
		where
			map_set = 'DaypartNetworks'
			and
			network_map.network_id <> daypart_network.id
		union all
		select
			daypart_network.id national_network_id,
			network_map.network_id daypart_network_id,
			network_map.effective_date start_date,
			'12/31/2032' end_date
		from
			network_maps network_map
			join networks daypart_network on
				daypart_network.id = network_map.map_value
		where
			map_set = 'DaypartNetworks'
			and
			network_map.network_id <> daypart_network.id
		union all
		select
			daypart_network.id national_network_id,
			network_map.network_id daypart_network_id,
			network_map.start_date,
			network_map.end_date
		from
			network_map_histories network_map
			join networks daypart_network on
				daypart_network.code = network_map.map_value
		where
			map_set = 'DaypartNetworks'
			and
			network_map.network_id <> daypart_network.id
	),
	all_traffic_maps(
		national_network_id, 
		regional_network_id, 
		start_date, 
		end_date
	) as (
		select
			network_map.network_id national_network_id,
			regional_network.id regional_network_id,
			network_map.effective_date start_date,
			'12/31/2032' end_date
		from
			network_maps network_map
			join networks regional_network on
				regional_network.code = network_map.map_value
		where
			map_set = 'Traffic'
--			map_set = 'PostReplace'
			and
			network_map.network_id <> regional_network.id
		union all
		select
			network_map.network_id national_network_id,
			regional_network.id regional_network_id,
			network_map.start_date,
			network_map.end_date
		from
			network_map_histories network_map
			join networks regional_network on
				regional_network.code = network_map.map_value
		where
			map_set = 'Traffic'
--			map_set = 'PostReplace'
			and
			network_map.network_id <> regional_network.id
	),
	all_network_substitutions(
		network_id, 
		substitution_category_id,
		substitute_network_id, 
		weight,
		start_date, 
		end_date
	) as (
		select
			network_substitution.network_id, 
			network_substitution.substitution_category_id,
			network_substitution.substitute_network_id, 
			network_substitution.weight, 
			network_substitution.effective_date start_date,
			'12/31/2032' end_date
		from
			network_substitutions network_substitution
		union all
		select
			network_substitution.network_id, 
			network_substitution.substitution_category_id,
			network_substitution.substitute_network_id, 
			network_substitution.weight, 
			network_substitution.start_date, 
			network_substitution.end_date
		from
			dbo.network_substitution_histories network_substitution
	),
	all_national_network_maps(
		network_id,
		nielsen_network_id,
		weight,
		start_date,
		end_date
	) as (
		select
			nielsen_map.network_id,
			nielsen_network.nielsen_network_id,
			1.0 weight,
			case
				when nielsen_map.start_date > nielsen_network.start_date then nielsen_map.start_date
				else nielsen_network.start_date
			end start_date,
			case
				when nielsen_map.end_date < nielsen_network.end_date then nielsen_map.end_date
				else nielsen_network.end_date
			end end_date
		from
			all_nielsen_maps nielsen_map
			join all_nielsen_networks nielsen_network on
				nielsen_network.nielsen_id = nielsen_map.nielsen_id
				and
				nielsen_network.start_date <= nielsen_map.end_date
				and
				nielsen_map.start_date <= nielsen_network.end_date
	)
	select
		traffic_network_map.regional_network_id network_id,
		network_substitution.substitution_category_id,
		substitute_network_map.nielsen_network_id,
		network_substitution.weight,
		case
			when 
				network_substitution.start_date > national_network_map.start_date 
				and
				network_substitution.start_date > traffic_network_map.start_date 
				then network_substitution.start_date
			when 
				traffic_network_map.start_date > national_network_map.start_date 
				then network_substitution.start_date
			else national_network_map.start_date
		end start_date,
		case
			when 
				network_substitution.end_date < national_network_map.end_date 
				and
				network_substitution.end_date < traffic_network_map.end_date 
				then network_substitution.end_date
			when 
				traffic_network_map.end_date < national_network_map.end_date 
				then network_substitution.end_date
			else national_network_map.end_date
		end end_date,
		'traffic_network_map' source
	from
		all_national_network_maps national_network_map
		join all_traffic_maps traffic_network_map on
			national_network_map.network_id = traffic_network_map.national_network_id
			and
			traffic_network_map.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= traffic_network_map.end_date
		join all_network_substitutions network_substitution on
			network_substitution.network_id = national_network_map.network_id
			and
			network_substitution.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= network_substitution.end_date
		join all_national_network_maps substitute_network_map on
			network_substitution.substitute_network_id = substitute_network_map.network_id
			and
			national_network_map.start_date <= substitute_network_map.end_date
			and
			substitute_network_map.start_date <= national_network_map.end_date
	union
	select
		daypart_network_map.national_network_id network_id,
		substitution_category.id substitution_category_id,
		isnull(substitute_network_map.nielsen_network_id, national_network_map.nielsen_network_id) nielsen_network_id,
		isnull(network_substitution.weight, national_network_map.weight) weight,
		case
			when 
				network_substitution.start_date > national_network_map.start_date 
				and
				network_substitution.start_date > daypart_network_map.start_date 
				then network_substitution.start_date
			when 
				daypart_network_map.start_date > national_network_map.start_date 
				then network_substitution.start_date
			else national_network_map.start_date
		end start_date,
		case
			when 
				network_substitution.end_date < national_network_map.end_date 
				and
				network_substitution.end_date < daypart_network_map.end_date 
				then network_substitution.end_date
			when 
				daypart_network_map.end_date < national_network_map.end_date 
				then network_substitution.end_date
			else national_network_map.end_date
		end end_date,
		'daypart_network_map' source
	from
		all_daypart_maps daypart_network_map
		join dbo.substitution_categories (nolock) substitution_category on
			'Delivery' = substitution_category.name
		join all_national_network_maps national_network_map on
			national_network_map.network_id = daypart_network_map.daypart_network_id
			and
			daypart_network_map.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= daypart_network_map.end_date
		left join all_network_substitutions network_substitution on
			network_substitution.network_id = national_network_map.network_id
			and
			network_substitution.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= network_substitution.end_date
		left join all_national_network_maps substitute_network_map on
			network_substitution.substitute_network_id = substitute_network_map.network_id
			and
			national_network_map.start_date <= substitute_network_map.end_date
			and
			substitute_network_map.start_date <= national_network_map.end_date
	union
	select
		national_network_map.network_id,
		network_substitution.substitution_category_id,
		substitute_network_map.nielsen_network_id,
		network_substitution.weight,
		case
			when network_substitution.start_date > national_network_map.start_date then network_substitution.start_date
			else national_network_map.start_date
		end start_date,
		case
			when network_substitution.end_date < national_network_map.end_date then network_substitution.end_date
			else national_network_map.end_date
		end end_date,
		'network_substitution' source
	from
		all_national_network_maps national_network_map
		join all_network_substitutions network_substitution on
			network_substitution.network_id = national_network_map.network_id
			and
			network_substitution.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= network_substitution.end_date
		join all_national_network_maps substitute_network_map on
			network_substitution.substitute_network_id = substitute_network_map.network_id
			and
			national_network_map.start_date <= substitute_network_map.end_date
			and
			substitute_network_map.start_date <= national_network_map.end_date
	union
	select
		national_network_map.network_id,
		substitution_category.id substitution_category_id,
		national_network_map.nielsen_network_id,
		national_network_map.weight,
		national_network_map.start_date,
		national_network_map.end_date,
		'national_network_map' source
	from
		all_national_network_maps national_network_map
		cross join dbo.substitution_categories (nolock) substitution_category
		left join all_network_substitutions network_substitution on
			network_substitution.network_id = national_network_map.network_id
			and
			network_substitution.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= network_substitution.end_date
		left join all_traffic_maps traffic_network_map on
			national_network_map.network_id = traffic_network_map.regional_network_id
			and
			traffic_network_map.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= traffic_network_map.end_date
	where
		network_substitution.network_id is null
		and
		traffic_network_map.national_network_id is null;
/*
select
	*
from
	uvw_network_2_nielsen_network_ratings_map;

select	
	*
From
	networks
where
	id = 47

select
	*
from
	nielsen_networks
where
	id in (37, 338)

*/
