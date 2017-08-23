CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastMarketUniverseByAudience]
(
	@audience_id int,
	@market_include_list varchar(max),
	@market_exclude_list varchar(max),
	@market_rank_include_list varchar(max),
	@market_rank_exclude_list varchar(max)
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
	
	select @audienceCode = a.name from dbo.audiences a with(nolock)
	where
		a.id = @audience_id;
	-- Get Audience String
	
	declare @query varchar(max);
	set @query = '';
	set @query = @query + 'SELECT distinct mu.market_code, mu.market_rank, mu.universe, 
	a.name from external_rating.dbo.udf_GetNSIMarketUniverses(''' + @codeMonth + ''', ''' + @audienceCode + ''', ''Total DMA'') mu ';
	set @query = @query + 'JOIN audiences a WITH (NOLOCK) on a.id = mu.audience_id ';
	
	IF(@market_rank_include_list IS NOT NULL)
	BEGIN
		set @query = @query + ' AND mu.market_rank <= ' + @market_rank_include_list;
	END
	
	IF(@market_rank_exclude_list IS NOT NULL)
	BEGIN
		set @query = @query + ' AND mu.market_rank >= ' + @market_rank_exclude_list;
	END
		
	IF @market_include_list IS NOT NULL
	BEGIN
		set @query = @query + ' AND mu.market_code in (' + @market_include_list + ')';
	END
	IF @market_exclude_list IS NOT NULL
	BEGIN
		set @query = @query + ' AND mu.market_code not in (' + @market_exclude_list + ')';	
	END
	
	set @query = @query + 'order by mu.market_rank;';
	
	print 'Q;'+ @query;
	exec (@query);

END
