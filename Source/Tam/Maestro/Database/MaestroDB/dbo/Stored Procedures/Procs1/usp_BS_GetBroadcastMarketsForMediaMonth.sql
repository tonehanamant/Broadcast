
CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastMarketsForMediaMonth]
(
	@media_month_id int
)
AS

if(@media_month_id is null)
BEGIN
-- GET THE LATEST NOVEMBER MONTH WE HAVE LOADED IN THE SYSTEM!
select @media_month_id = max(distinct mh.media_month_id) FROM external_rating.nsi.market_headers mh WITH (NOLOCK)
		join media_months mm WITH (NOLOCK) on mm.id = mh.media_month_id
where mm.month = 11
END

	select distinct 
		dmap.map_value,
		mh.market_rank,
		mh.market_code
	from 
		external_rating.nsi.market_headers mh WITH (NOLOCK)
		join dma_maps dmap WITH (NOLOCK) on dmap.dma_id = mh.dma_id and dmap.map_set = 'Strata'
	where
		mh.media_month_id = @media_month_id
	ORDER BY
		mh.market_rank
