-- =============================================
-- Author:		Mike Deaven
-- Create date: 9/24/2012
-- Description:	Gets data for the expert bias report
-- =============================================
CREATE PROCEDURE usp_ARSLoader_GetExpertBiasReport
	@baseDate DATETIME,
	@forecastDate DATETIME,
	@ratingCategoryCode VARCHAR(15)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	declare
		@audience varchar(15),
		@daypart varchar(63);

	set @audience = 'hh';
	set @daypart = 'M-Su 6am-12am';
	
	declare 
		@lBase_media_month_id int,
		@lRBMedia_month_id int,
		@lEBMedia_month_id int,
		@rbSpotLengthID int,
		@hhAudienceID int,
		@fProposalRatingAdjustment float,
		@ratingCategoryId INT;
	
	set @fProposalRatingAdjustment = cast(1 as float);
	
	select 
		@lBase_media_month_id = id,
		@lRBMedia_month_id = id,
		@lEBMedia_month_id = id
	from 
		media_months 
	where 
		@baseDate between start_date and end_date;
	
	select
		@rbSpotLengthID = id 
	from
		spot_lengths 
	where
		length = 30;
	
	set @hhAudienceID = dbo.GetIDFromAudienceString( 'HH' );
	
	SELECT @ratingCategoryId = id
	FROM rating_categories
	WHERE code = @ratingCategoryCode;
	
	with
	nnn(
		network_id,
		nielsen_network_id
	) as (
		select
			n.id network_id,
			nn.id nielsen_network_id
		from
			networks n
			join network_maps nm on
				n.id = nm.network_id
				and
				'Nielsen' = nm.map_set
			join nielsen_networks nn on
				nn.nielsen_id = cast(nm.map_value as int)
	),
	nh_h(
		network_id
	) as (
		select
			id
		from
			networks
		where
			code like '%-h'
	),
	nh_us(
		network_id
	) as (
		select
			n_us.id
		from
			nh_h
			join networks n_h on
				n_h.id = nh_h.network_id
			join networks n_us on
				n_us.code = left(n_h.code, len(n_h.code) - 2)
	),
	nh_a(
		network_id
	) as (
		select
			network_id
		from
			nh_us
		union
		select
			network_id
		from
			nh_h
	),
	rcn(
		network_id
	) as (
		select distinct
			network_id
		from
			network_rate_card_details
		union
		select
			id network_id
		from
			networks
		where
			code in ('amone', 'DKID', 'FUSE', 'G4', 'insp', 'mlb', 'real', 'rltv','TV1', 'BBCA', 'FBN', 'MIL')
	),
	rcnn(
		nielsen_network_id
	) as (
		select
			nnn.nielsen_network_id
		from
			rcn
			join nnn on
				rcn.network_id = nnn.network_id
		union
		select
			nn.id
		from	
			nielsen_networks nn
		where
			nn.code in ('TotUS')
	),
	fmm(
		media_month_id,
		media_month,
		start_date,
		end_date,
		weeks
	) as (
		select 
			mm.id,
			mm.media_month,
			mm.start_date,
			mm.end_date,
			count(*) weeks 
		from 
			media_months mm
			join media_weeks mw on
				mm.id = mw.media_month_id
		where 
			@forecastDate BETWEEN mm.start_date AND mm.end_date
		group by
			mm.id,
			mm.media_month,
			mm.start_date,
			mm.end_date
	),
	nsa(
		network_id,
		substitution_category_id,
		substitute_network_id,
		weight,
		start_date,
		end_date
	) as (
		select
			network_id,
			substitution_category_id,
			substitute_network_id,
			weight,
			effective_date start_date,
			'12/31/2032' end_date
		from
			network_substitutions ns
	
		union
	
		select
			network_id,
			substitution_category_id,
			substitute_network_id,
			weight,
			start_date,
			end_date
		from
			network_substitution_histories ns
	),
	fmns(
			nielsen_network_id,
			substitution_category,
			substitute_nielsen_network_id,
			weight,
			start_date,
			end_date
	) as (
		select
			net_nn.nielsen_network_id nielsen_network_id,
			sc.name substitution_category,
			sub_nn.nielsen_network_id substitute_nielsen_network_id,
			ns.weight,
			ns.start_date,
			ns.end_date
		from
			nsa ns
			join substitution_categories sc on
				sc.id = ns.substitution_category_id
			join nnn net_nn on
				ns.network_id = net_nn.network_id
			join nnn sub_nn on
				ns.substitute_network_id = sub_nn.network_id
	),
	fmdp(
		daypart_id,
		daypart_text
	) as (
		select 
			dp.id, 
			dp.daypart_text
		from 
			dayparts dp
		where 
			@daypart = dp.daypart_text
	),
	fma(
		fm_audience_id,
		fm_name,
		fm_sort_position,
		rating_audience_id
	) as (
	select
		aa.custom_audience_id,	
		fm_a_name.map_value,
		cast(fm_a_sort.map_value as int),
		aa.rating_audience_id
	from
		audience_audiences aa
		join audience_maps fm_a_name on 
			aa.custom_audience_id = fm_a_name.audience_id
			and
			'FileMaker' = fm_a_name.map_set
		join audience_maps fm_a_sort on 
			aa.custom_audience_id = fm_a_sort.audience_id
			and
			'fmSortOrder' = fm_a_sort.map_set
	where
		@hhAudienceID = aa.custom_audience_id
	
	),
	fmexp(
		nielsen_network_id,
		network_rating_category_id,
		audience_id,
		daypart_id,
		daypart_text,
		network,
		dma_code,
		audience,
		audience_order
	) as (
		select distinct
			nn.id,
			nn.network_rating_category_id,
			fma.fm_audience_id,
			fmdp.daypart_id,
			fmdp.daypart_text,
			case
				when nn.code like '%-H' then left(nn.code,len(nn.code)-2)
				else nn.code
			end network,
			case
				when nn.code = 'FXSP' then -7401
				when nn.nielsen_id < 0 then -nn.nielsen_id
				else nn.nielsen_id
			end dma_code,
			fma.fm_name,
			fma.fm_sort_position
		from
			nielsen_networks nn,
			fma,
			fmdp
		where
			nn.id in (
				select
					nielsen_network_id
				from
					rcnn
			)
	),
	fmunv(
		nielsen_network_id, 
		audience_id, 
		forecast
	) as (
		select 
			u.nielsen_network_id, 
			fma.fm_audience_id, 
			(sum(u.universe * fmm.weeks) / (select sum(weeks) from fmm)) * 0.001 [forecast]
		from 
			universes u
			join fma on 
				u.audience_id = fma.rating_audience_id
			join fmm on
				fmm.media_month_id = u.forecast_media_month_id
			left join fmns on
				fmns.nielsen_network_id = u.nielsen_network_id
				and
				'Universe' = fmns.substitution_category
				and
				fmm.start_date between fmns.start_date and fmns.end_date
		where
			fmns.nielsen_network_id is null
			and u.base_media_month_id = @lBase_media_month_id
			AND u.rating_category_id = @ratingCategoryId
		group by 
			u.nielsen_network_id,
			fma.fm_audience_id
	
		union
	
		select 
			fmns.nielsen_network_id, 
			fma.fm_audience_id, 
			(sum(u.universe * fmm.weeks) / (select sum(weeks) from fmm)) * 
				max(fmns.weight) *
				0.001 [forecast]
		from 
			universes u
			join fma on 
				u.audience_id = fma.rating_audience_id
			join fmm on
				fmm.media_month_id = u.forecast_media_month_id
			join fmns on
				fmns.substitute_nielsen_network_id = u.nielsen_network_id
				and
				'Universe' = fmns.substitution_category
				and
				fmm.start_date between fmns.start_date and fmns.end_date
		where
			u.base_media_month_id = @lBase_media_month_id
			AND u.rating_category_id = @ratingCategoryId
		group by 
			fmns.nielsen_network_id, 
			fma.fm_audience_id
	),
	fmdel(
		nielsen_network_id, 
		audience_id, 
		daypart_id, 
		forecast,
		rotational_bias,
		expert_bias,
		c3_bias
	) as (
		select 
			r.nielsen_network_id, 
			fma.fm_audience_id, 
			fmdp.daypart_id, 
			(sum(r.audience_usage * fmm.weeks) / (select sum(weeks) from fmm)) * 0.001 [forecast],
			avg(isnull(rb.bias,1)) [rotational_bias], 
			avg(isnull(eb.bias,1)) [expert_bias],
			avg(isnull(c3b.bias,1)) [c3_bias]
		from 
			ratings r
			--ratings r
			join fmm on
				fmm.media_month_id = r.forecast_media_month_id
			join fma on 
				r.audience_id = fma.rating_audience_id
			join fmdp on 
				fmdp.daypart_id = r.daypart_id
			left join fmns on
				fmns.nielsen_network_id = r.nielsen_network_id
				and
				'Delivery' = fmns.substitution_category
				and
				fmm.start_date between fmns.start_date and fmns.end_date
			left join rotational_biases rb on 
				r.nielsen_network_id = rb.nielsen_network_id 
				and
				r.daypart_id = rb.daypart_id
				and
				@rbSpotLengthID = rb.spot_length_id
				and
				@hhAudienceID = rb.audience_id
				and
				@lRBMedia_month_id = rb.media_month_id
			left join expert_biases eb on 
				eb.media_month_id = @lEBMedia_month_id 
				and 
				eb.nielsen_network_id = r.nielsen_network_id 
				and 
				@hhAudienceID = eb.audience_id
			left join c3_biases c3b on 
				r.nielsen_network_id = c3b.nielsen_network_id 
				and
				r.daypart_id = c3b.daypart_id
				and
				@hhAudienceID = c3b.audience_id
				and
				@lBase_media_month_id = c3b.media_month_id
		where
			fmns.nielsen_network_id is null
			and
			r.base_media_month_id = @lBase_media_month_id
			AND r.rating_category_id = @ratingCategoryId
		group by 
			r.nielsen_network_id, 
			fmdp.daypart_id,
			fma.fm_audience_id
	
		union
	
		select 
			fmns.nielsen_network_id, 
			fma.fm_audience_id, 
			fmdp.daypart_id, 
			(sum(r.audience_usage * fmm.weeks * fmns.weight) / (select sum(weeks) from fmm)) * 0.001 [forecast],
			avg(isnull(rb.bias,1)) [rotational_bias], 
			avg(isnull(eb.bias,1)) [expert_bias],
			avg(isnull(c3b.bias,1)) [c3_bias]
		from 
			ratings r
			--ratings r
			join fmm on
				fmm.media_month_id = r.forecast_media_month_id
			join fmns on
				fmns.substitute_nielsen_network_id = r.nielsen_network_id
				and
				'Delivery' = fmns.substitution_category
				and
				fmm.start_date between fmns.start_date and fmns.end_date
			join fma on 
				r.audience_id = fma.rating_audience_id
			join fmdp on 
				fmdp.daypart_id = r.daypart_id
			left join rotational_biases rb on 
				fmns.nielsen_network_id = rb.nielsen_network_id 
				and
				r.daypart_id = rb.daypart_id
				and
				@rbSpotLengthID = rb.spot_length_id
				and
				@hhAudienceID = rb.audience_id
				and
				@lRBMedia_month_id = rb.media_month_id
			left join expert_biases eb on 
				eb.media_month_id = @lEBMedia_month_id 
				and 
				eb.nielsen_network_id = fmns.nielsen_network_id 
				and 
				@hhAudienceID = eb.audience_id
			left join c3_biases c3b on 
				fmns.nielsen_network_id = c3b.nielsen_network_id 
				and
				r.daypart_id = c3b.daypart_id
				and
				@hhAudienceID = c3b.audience_id
				and
				@lBase_media_month_id = c3b.media_month_id
		where
			r.base_media_month_id = @lBase_media_month_id
			AND r.rating_category_id = @ratingCategoryId
		group by 
			fmns.nielsen_network_id, 
			fmdp.daypart_id,
			fma.fm_audience_id
	),
	fmud(
		nielsen_network_id, 
		audience_id, 
		daypart_id, 
		universe,
		delivery,
	--	raw_rating,
		rotational_bias,
		expert_bias,
		c3_bias--,
	--	final_rating
	) as (
		select
			r.nielsen_network_id, 
			r.audience_id, 
			r.daypart_id, 
			u.forecast universe,
			r.forecast delivery,
	--		r.forecast / u.forecast raw_rating,
			r.rotational_bias,
			r.expert_bias,
			r.c3_bias--,
	--		(r.forecast / u.forecast) * r.rotational_bias * r.expert_bias * r.c3_bias final_rating
		from
			fmdel r
			full join fmunv u on
				r.nielsen_network_id = u.nielsen_network_id
				and
				r.audience_id = u.audience_id
	),
	aunv(
		nielsen_network_id, 
		audience_id, 
		actual
	) as (
		select 
			u.nielsen_network_id, 
			fma.fm_audience_id, 
			(sum(u.universe * fmm.weeks) / (select sum(weeks) from fmm)) * 0.001 [actual]
		from 
			universes u
			join fma on 
				u.audience_id = fma.rating_audience_id
			join fmm on
				fmm.media_month_id = u.forecast_media_month_id
			left join fmns on
				fmns.nielsen_network_id = u.nielsen_network_id
				and
				'Universe' = fmns.substitution_category
				and
				fmm.start_date between fmns.start_date and fmns.end_date
		where
			fmns.nielsen_network_id is null
			and
			u.base_media_month_id = u.forecast_media_month_id
			AND u.rating_category_id = @ratingCategoryId
		group by 
			u.nielsen_network_id,
			fma.fm_audience_id
	
		union
	
		select 
			fmns.nielsen_network_id, 
			fma.fm_audience_id, 
			(sum(u.universe * fmm.weeks) / (select sum(weeks) from fmm)) * 
				max(fmns.weight) *
				0.001 [actual]
		from 
			universes u 
			join fma on 
				u.audience_id = fma.rating_audience_id
			join fmm on
				fmm.media_month_id = u.forecast_media_month_id
			join fmns on
				fmns.substitute_nielsen_network_id = u.nielsen_network_id
				and
				'Universe' = fmns.substitution_category
				and
				fmm.start_date between fmns.start_date and fmns.end_date
		where
			u.base_media_month_id = u.forecast_media_month_id
			AND u.rating_category_id = @ratingCategoryId
		group by 
			fmns.nielsen_network_id, 
			fma.fm_audience_id
	),
	adel(
		nielsen_network_id, 
		audience_id, 
		daypart_id, 
		actual
	) as (
		select 
			r.nielsen_network_id, 
			fma.fm_audience_id, 
			fmdp.daypart_id, 
			(sum(r.audience_usage * fmm.weeks) / (select sum(weeks) from fmm)) * 0.001 [actual]
		from 
			ratings r
			--ratings r
			join fmm on
				fmm.media_month_id = r.forecast_media_month_id
			join fma on 
				r.audience_id = fma.rating_audience_id
			join fmdp on 
				fmdp.daypart_id = r.daypart_id
			left join fmns on
				fmns.nielsen_network_id = r.nielsen_network_id
				and
				'Delivery' = fmns.substitution_category
				and
				fmm.start_date between fmns.start_date and fmns.end_date
			left join rotational_biases rb on 
				r.nielsen_network_id = rb.nielsen_network_id 
				and
				r.daypart_id = rb.daypart_id
				and
				@rbSpotLengthID = rb.spot_length_id
				and
				@hhAudienceID = rb.audience_id
				and
				@lRBMedia_month_id = rb.media_month_id
			left join expert_biases eb on 
				eb.media_month_id = @lEBMedia_month_id 
				and 
				eb.nielsen_network_id = r.nielsen_network_id 
				and 
				@hhAudienceID = eb.audience_id
			left join c3_biases c3b on 
				r.nielsen_network_id = c3b.nielsen_network_id 
				and
				r.daypart_id = c3b.daypart_id
				and
				@hhAudienceID = c3b.audience_id
				and
				@lBase_media_month_id = c3b.media_month_id
		where
			fmns.nielsen_network_id is null
			and
			r.base_media_month_id = r.forecast_media_month_id
			AND r.rating_category_id = @ratingCategoryId
		group by 
			r.nielsen_network_id, 
			fmdp.daypart_id,
			fma.fm_audience_id
	
		union
	
		select 
			fmns.nielsen_network_id, 
			fma.fm_audience_id, 
			fmdp.daypart_id, 
			(sum(r.audience_usage * fmm.weeks * fmns.weight) / (select sum(weeks) from fmm)) * 0.001 [actual]
		from 
			ratings r
			--ratings r
			join fmm on
				fmm.media_month_id = r.forecast_media_month_id
			join fmns on
				fmns.substitute_nielsen_network_id = r.nielsen_network_id
				and
				'Delivery' = fmns.substitution_category
				and
				fmm.start_date between fmns.start_date and fmns.end_date
			join fma on 
				r.audience_id = fma.rating_audience_id
			join fmdp on 
				fmdp.daypart_id = r.daypart_id
			left join rotational_biases rb on 
				fmns.nielsen_network_id = rb.nielsen_network_id 
				and
				r.daypart_id = rb.daypart_id
				and
				@rbSpotLengthID = rb.spot_length_id
				and
				@hhAudienceID = rb.audience_id
				and
				@lRBMedia_month_id = rb.media_month_id
			left join expert_biases eb on 
				eb.media_month_id = @lEBMedia_month_id 
				and 
				eb.nielsen_network_id = fmns.nielsen_network_id 
				and 
				@hhAudienceID = eb.audience_id
			left join c3_biases c3b on 
				fmns.nielsen_network_id = c3b.nielsen_network_id 
				and
				r.daypart_id = c3b.daypart_id
				and
				@hhAudienceID = c3b.audience_id
				and
				@lBase_media_month_id = c3b.media_month_id
		where
			r.base_media_month_id = r.forecast_media_month_id
			AND r.rating_category_id = @ratingCategoryId
		group by 
			fmns.nielsen_network_id, 
			fmdp.daypart_id,
			fma.fm_audience_id
	),
	aud(
		nielsen_network_id, 
		audience_id, 
		daypart_id, 
		universe,
		delivery,
		rating
	) as (
		select
			r.nielsen_network_id, 
			r.audience_id, 
			r.daypart_id, 
			u.actual universe,
			r.actual delivery,
			r.actual / u.actual rating
		from
			adel r
			full join aunv u on
				r.nielsen_network_id = u.nielsen_network_id
				and
				r.audience_id = u.audience_id
	),
	dr(
		nielsen_network_id,
		rating
	) as (
		select
			nnn.nielsen_network_id,
			avg(dr.rating) rating
		from
			delivered_ratings dr
			join nnn on
				nnn.network_id = dr.network_id
			join fmm on
				fmm.media_month_id = dr.media_month_id
		group by
			nnn.nielsen_network_id
	)

	select
		fmexp.network [Network],
		fmexp.dma_code [DMACode],
		isnull(fmud.universe,0.0) [Universe],
		isnull(fmud.delivery,0.0) * @fProposalRatingAdjustment [Raw HH (000)],
		case
			when isnull(fmud.universe,0.0) = 0.0 then null
			else (isnull(fmud.delivery,0.0) * @fProposalRatingAdjustment) / isnull(fmud.universe,0.0)
		end [Raw Rtg],
		isnull(fmud.rotational_bias,1) [Rotational Bias],
		isnull(fmud.expert_bias,1) [Expert Bias],
	--	isnull(fmud.c3_bias,1) [C3 Bias],
		fmud.delivery
			* isnull(fmud.rotational_bias,1) 
			* isnull(fmud.expert_bias,1)
	--		* isnull(fmud.c3_bias,1)
		[Final HH (000)],
		(fmud.delivery / fmud.universe) 
			* isnull(fmud.rotational_bias,1) 
			* isnull(fmud.expert_bias,1)
	--		* isnull(fmud.c3_bias,1)
		[Rating]
	from
		fmexp
		left join network_rating_categories nrc on
			nrc.id = fmexp.network_rating_category_id
		left join dr on
			dr.nielsen_network_id = fmexp.nielsen_network_id
		left join fmud on
			fmud.nielsen_network_id = fmexp.nielsen_network_id
			and
			fmud.audience_id = fmexp.audience_id
			and
			fmud.daypart_id = fmexp.daypart_id
		left join aud on
			aud.nielsen_network_id = fmexp.nielsen_network_id
			and
			aud.audience_id = fmexp.audience_id
			and
			aud.daypart_id = fmexp.daypart_id
	where
		fmud.universe is not null
		and
		fmexp.network in (
			'ADSM',
			'AEN',
			'AMC',
			'APL',
			'BET',
			'BIO',
			'BRVO',
			'BTV',
			'CMD',
			'CMTV',
			'CNB',
			'CNN',
			'DISC',
			'DIY',
			'ENT',
			'ESP2',
			'ESPN',
			'FAM',
			'FBN',
			'FINE',
			'FOOD',
			'FX',
			'FXNC',
			'FXSP',
			'G4',
			'GOLF',
			'GSN',
			'HALL',
			'HGTV',
			'HIS',
			'HLN',
			'INSP',
			'ION',
			'LIF',
			'LMN',
			'MNBC',
			'MTV',
			'MTV2',
			'NAN',
			'NBCSN',
			'NFLN',
			'NGC',
			'NICK',
			'OWN',
			'OXYG',
			'SC',
			'SFI',
			'SOAP',
			'SPK',
			'STYL',
			'TBSC',
			'TDSY',
			'TLC',
			'TNT',
			'TOON',
			'TRAV',
			'TRU',
			'TV1',
			'TVGC',
			'TVL',
			'TWC',
			'USA',
			'VH1',
			'VS',
			'WE',
			'WGNC',
			'TotUS', 
			'BBCA', 
			'MIL'
		)
	order by 
		audience_order,
		fmexp.daypart_text,
		case fmexp.network
			when 'TotUS' then 'ZZZZZ'
			else fmexp.network
		end;
END
