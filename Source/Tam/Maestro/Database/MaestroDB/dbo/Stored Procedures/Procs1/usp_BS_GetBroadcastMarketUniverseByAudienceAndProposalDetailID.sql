


CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastMarketUniverseByAudienceAndProposalDetailID]
(
	@proposal_detail_id int,
	@audience_id int
)
AS
BEGIN 
	-- We need to use the latest sweeps month
	declare @media_month_id int;
	declare @codeMonth varchar(7);
	declare @audienceCode varchar(7);
	
	select @media_month_id = max(distinct mh.media_month_id) FROM external_rating.nsi.market_headers mh WITH (NOLOCK)
			join media_months mm WITH (NOLOCK) on mm.id = mh.media_month_id
	where mm.month = 11;
	
	SELECT @codeMonth = REPLICATE(0, 2-len(CONVERT(varchar(2), mm.month))) + CONVERT(Varchar(255), mm.month) + 
	SUBSTRING(cast(mm.year as varchar), 3, 2) from 
	media_months mm WITH (NOLOCK) 
	WHERE mm.id = @media_month_id;
	
	select @audienceCode = a.name from audiences a with(nolock)
	where
		a.id = @audience_id;
	
	-- Get Audience String
	SELECT 
		distinct 
			mu.market_code, 
			mu.market_rank, 
			mu.universe, 
			dh.demographic 
		from 
			external_rating.dbo.udf_GetNSIMarketUniverses(@codeMonth,@audienceCode, 'Total DMA') mu 
			JOIN external_rating.nsi.demographic_headers dh WITH (NOLOCK) on dh.audience_id = mu.audience_id 
			JOIN external_rating.dbo.program_monthly_schedule pmc WITH (NOLOCK) on pmc.media_month_id = @media_month_id and pmc.market_code = mu.market_code
			join zone_maps zm1 with (NOLOCK) on zm1.map_value = pmc.station_code and zm1.map_set = 'Brdcst Callltr'	
			join broadcast_traffic_details bptd with (NOLOCK) on bptd.zone_id = zm1.zone_id
		where
			bptd.broadcast_proposal_detail_id = @proposal_detail_id
		order by 
			mu.market_rank
	

END

