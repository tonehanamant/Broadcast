CREATE view [dbo].[uvw_nielsen_network_substitutions] as 
	select
		nn.id nielsen_network_id,
		ns.substitution_category_id,
		s_nn.id substitute_nielsen_network_id,
		ns.weight
	from
		network_substitutions ns
		join networks n on
			n.id = ns.network_id
		join networks s_n on
			s_n.id = ns.substitute_network_id
		join substitution_categories sc on
			sc.id = ns.substitution_category_id
		join network_maps nm on
			n.id = nm.network_id
			and
			'nielsen' = nm.map_set
		join nielsen_networks nn on
			nm.map_value = nn.nielsen_id
		join network_maps s_nm on
			s_n.id = s_nm.network_id
			and
			'nielsen' = s_nm.map_set
		join nielsen_networks s_nn on
			s_nm.map_value = s_nn.nielsen_id;
