


CREATE PROCEDURE [dbo].[usp_REC_GetNetworksForTier]
(
            @tier as int
)

AS

with maxid (id, media_month_id) 
	as (
	select top 1 id, max(base_ratings_media_month_id) 
		from network_rate_card_books  (NOLOCK)
	where sales_model_id = 1
		group by id
	order by max(base_ratings_media_month_id) DESC
)
select network_rate_card_details.network_id, networks.code 
	from network_rate_card_details (NOLOCK) 
		join networks (NOLOCK) on networks.id = network_rate_card_details.network_id
		join network_rate_cards (NOLOCK) on network_rate_cards.id = network_rate_card_details.network_rate_card_id
		join maxid on network_rate_cards.network_rate_card_book_id = maxid.id
	where network_rate_cards.daypart_id = 1 
		and network_rate_cards.rate_card_type_id = 1
		and network_rate_card_details.tier = @tier
	order by networks.code


