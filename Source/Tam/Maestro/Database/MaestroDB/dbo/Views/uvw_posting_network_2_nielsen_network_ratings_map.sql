-- =============================================
-- Author:		David Sissoin
--				Modified by Steve DeFusco
-- Create date: 
-- Description:	Used for calculating delivery.
-- =============================================
CREATE VIEW [dbo].[uvw_posting_network_2_nielsen_network_ratings_map] AS 
	with
	hispanic_networks(
		network_id
	) as (
		select distinct
			network_id
		from
			hispanic_post_network_substitutions (NOLOCK)
	),
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
			network_maps (NOLOCK)
		where
			map_set = 'Nielsen'
		union all
		select
			network_id,
			cast(map_value as int) nielsen_id,
			start_date,
			end_date
		from
			network_map_histories (NOLOCK)
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
			nielsen_networks (NOLOCK)
		union
		select
			nielsen_network_id,
			nielsen_id,
			start_date,
			end_date
		from
			nielsen_network_histories (NOLOCK)
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
			network_maps network_map (NOLOCK)
			join networks daypart_network (NOLOCK) on
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
			network_map_histories network_map (NOLOCK)
			join networks daypart_network (NOLOCK) on
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
			network_maps network_map (NOLOCK)
			join networks daypart_network (NOLOCK) on
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
			network_map_histories network_map (NOLOCK)
			join networks daypart_network (NOLOCK) on
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
			network_maps network_map (NOLOCK)
			join networks regional_network (NOLOCK) on
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
			network_map_histories network_map (NOLOCK)
			join networks regional_network (NOLOCK) on
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
		end_date,
		rating_category_group_id
	) as (
		select
			network_substitution.network_id, 
			network_substitution.substitution_category_id,
			network_substitution.substitute_network_id, 
			network_substitution.weight, 
			network_substitution.effective_date start_date,
			'12/31/2032' end_date,
			network_substitution.rating_category_group_id
		from
			network_substitutions network_substitution (NOLOCK)
		where
			network_substitution.network_id not in (
				select
					hispanic_networks.network_id
				from
					hispanic_networks
			)
		union all
		select
			network_substitution.network_id, 
			network_substitution.substitution_category_id,
			network_substitution.substitute_network_id, 
			network_substitution.weight, 
			network_substitution.start_date, 
			network_substitution.end_date,
			network_substitution.rating_category_group_id
		from
			dbo.network_substitution_histories network_substitution (NOLOCK)
		where
			network_substitution.network_id not in (
				select
					hispanic_networks.network_id
				from
					hispanic_networks
			)
		union all
		select
			network_substitution.network_id, 
			network_substitution.substitution_category_id,
			network_substitution.substitute_network_id, 
			network_substitution.weight, 
			network_substitution.effective_date start_date,
			'12/31/2032' end_date,
			network_substitution.rating_category_group_id
		from
			hispanic_post_network_substitutions network_substitution (NOLOCK)
		where
			network_substitution.network_id in (
				select
					hispanic_networks.network_id
				from
					hispanic_networks
			)
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
			all_nielsen_maps nielsen_map (NOLOCK)
			join all_nielsen_networks nielsen_network (NOLOCK) on
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
		'traffic_network_map' source,
		network_substitution.rating_category_group_id
	from
		all_national_network_maps national_network_map (NOLOCK)
		join all_traffic_maps traffic_network_map (NOLOCK) on
			national_network_map.network_id = traffic_network_map.national_network_id
			and
			traffic_network_map.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= traffic_network_map.end_date
		join all_network_substitutions network_substitution (NOLOCK) on
			network_substitution.network_id = national_network_map.network_id
			and
			network_substitution.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= network_substitution.end_date
		join all_national_network_maps substitute_network_map (NOLOCK) on
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
		'daypart_network_map' source,
		network_substitution.rating_category_group_id
	from
		all_daypart_maps daypart_network_map (NOLOCK)
		join dbo.substitution_categories (NOLOCK) substitution_category on
			'Delivery' = substitution_category.name
		join all_national_network_maps national_network_map (NOLOCK) on
			national_network_map.network_id = daypart_network_map.daypart_network_id
			and
			daypart_network_map.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= daypart_network_map.end_date
		left join all_network_substitutions network_substitution (NOLOCK) on
			network_substitution.network_id = national_network_map.network_id
			and
			network_substitution.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= network_substitution.end_date
		left join all_national_network_maps substitute_network_map (NOLOCK) on
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
		'network_substitution' source,
		network_substitution.rating_category_group_id
	from
		all_national_network_maps national_network_map (NOLOCK)
		join all_network_substitutions network_substitution (NOLOCK) on
			network_substitution.network_id = national_network_map.network_id
			and
			network_substitution.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= network_substitution.end_date
		join all_national_network_maps substitute_network_map (NOLOCK) on
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
		'national_network_map' source,
		network_substitution.rating_category_group_id
	from
		all_national_network_maps national_network_map (NOLOCK)
		cross join dbo.substitution_categories (NOLOCK) substitution_category
		left join all_network_substitutions network_substitution on
			network_substitution.network_id = national_network_map.network_id
			and
			network_substitution.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= network_substitution.end_date
		left join all_traffic_maps traffic_network_map (NOLOCK) on
			national_network_map.network_id = traffic_network_map.regional_network_id
			and
			traffic_network_map.start_date <= national_network_map.end_date
			and
			national_network_map.start_date <= traffic_network_map.end_date
	where
		network_substitution.network_id is null
		and
		traffic_network_map.national_network_id is null;
