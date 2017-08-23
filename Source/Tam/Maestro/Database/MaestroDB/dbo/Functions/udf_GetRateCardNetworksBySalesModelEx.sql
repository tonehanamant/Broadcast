
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns the set of networks on the rate cards of the given sales model.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetRateCardNetworksBySalesModelEx]
(	
	@nameSalesModel as varchar(63),
	@excludeZeroRateNetworks as bit
)
RETURNS TABLE
AS
RETURN 
(
	select 
		n.id network_id, 
		nn.id nielsen_network_id,
		nn.nielsen_id nielsen_id,
		max(nrcd.tier) tier
	from 
		network_rate_card_rates nrcr with(nolock)
		join network_rate_card_details nrcd with(nolock) on 
			nrcd.id=nrcr.network_rate_card_detail_id
		join network_rate_cards nrc with(nolock) on 
			nrc.id=nrcd.network_rate_card_id
		join network_rate_card_books nrcb with(nolock) on 
			nrcb.id=nrc.network_rate_card_book_id
		join networks n with(nolock) on 
			n.id=nrcd.network_id
		--join dayparts d with(nolock) on d.id=nrc.daypart_id
		--join rate_card_types rct with(nolock) on rct.id=nrc.rate_card_type_id
		--join spot_lengths sl with(nolock) on sl.id = nrcr.spot_length_id
		join network_maps nm with(nolock) on 
			n.id = nm.network_id and 'Nielsen' = nm.map_set
		join nielsen_networks nn with(nolock) on 
			cast(nm.map_value as int) = nn.nielsen_id
		join sales_models sm with(nolock) on 
			sm.id = nrcb.sales_model_id
	where
		(
			@nameSalesModel is null
			or
			@nameSalesModel = sm.name
		)
		and
		(
			isnull(@excludeZeroRateNetworks, 0) = 0
			or
			nrcr.rate > 0
			--nrcd.hh_cpm > 0
		)
	group by
		n.id, 
		nn.id,
		nn.nielsen_id
);



