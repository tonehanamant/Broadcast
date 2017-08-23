
-- =============================================
-- Author:		Mike Deaven
-- Create date: 6/10/2014
-- Description:	Calculate Rotational Biases using the 4 year average
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARSLoader_CalculateRotationalBiases4Year]
	@baseMediaMonth varchar(15),
	@mediaMonth varchar(15),
	@ratingCategoryId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result SETs FROM
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	declare
		@isLive bit,
		@baseMonthID int,
		@monthID int,
		@monthIDm1 int,
		@monthIDm2 int,
		@monthIDm3 int,
		@month AS datetime,
		@rbSpotLengthID int,
		@hhAudienceID int;


	-- If the rating category is not live, skip everything
	SELECT
		@isLive = is_live
	FROM
		rating_categories
	WHERE
		id = @ratingCategoryId

	IF (@isLive = 0)
		RETURN
	
	SELECT
		@baseMonthID = id
	FROM
		media_months
	WHERE
		media_month = @baseMediaMonth;
	
	SELECT
		@monthID = id
	FROM
		media_months
	WHERE
		media_month = @mediaMonth;
	
	SET @month = dateadd(day,15,(SELECT start_date FROM media_months WHERE @monthID = id));
	
	SELECT
		@monthIDm1 = id
	FROM
		media_months
	WHERE
		dateadd(year, -1, @month) between start_date AND end_date;
	
	SELECT
		@monthIDm2 = id
	FROM
		media_months
	WHERE
		dateadd(year, -2, @month) between start_date AND end_date;
		
	SELECT
		@monthIDm3 = id
	FROM
		media_months
	WHERE
		dateadd(year, -3, @month) between start_date AND end_date;
	
	SELECT
		@rbSpotLengthID = id 
	FROM
		spot_lengths 
	WHERE
		length = 30;
	
	SET @hhAudienceID = dbo.GetIDFROMAudienceString('HH');
	
	with
	fmdp(
		daypart_id,
		fm_daypart_id
	) AS (
		SELECT
			dp.id,
			cast(dpm.map_value AS int) fm_daypart_id
		FROM
			dayparts dp
			JOIN daypart_maps dpm
			ON dp.id = dpm.daypart_id
		WHERE
			'FM_nDyptNum' = dpm.map_SET
			AND cast(dpm.map_value AS int) IN (1, 4, 5)
	),
	rbmm(
		media_month_id,
		media_month,
		start_date,
		end_date,
		weeks
	) AS (
		SELECT 
			mm.id,
			mm.media_month,
			mm.start_date,
			mm.end_date,
			count(*) weeks 
		FROM 
			media_months mm
			JOIN media_weeks mw
				ON mm.id = mw.media_month_id
		WHERE 
			mm.id IN (@monthID, @monthIDm1, @monthIDm2, @monthIDm3)
		GROUP BY
			mm.id,
			mm.media_month,
			mm.start_date,
			mm.end_date
	),
	n_nn(
		network_id,
		nielsen_network_id
	) AS (
		SELECT
			n.id network_id,
			nn.id nielsen_network_id
		FROM
			networks n
			JOIN network_maps nm
				ON 'nielsen' = nm.map_SET
				AND n.id = nm.network_id
			JOIN nielsen_networks nn
				ON nn.nielsen_id = nm.map_value
			LEFT JOIN dbo.network_maps nm2
				ON nm2.map_SET = 'RotBias'
				AND nm2.network_id = n.id
		WHERE nm2.map_value = '4Year'
	),
	nn_dr(
		media_month_id,
		nielsen_network_id,
		rating
	) AS (
		SELECT
			dr.media_month_id,
			n_nn.nielsen_network_id,
			dr.rating
		FROM
			n_nn 
			JOIN delivered_ratings dr
				ON dr.media_month_id IN (@monthID, @monthIDm1, @monthIDm2, @monthIDm3)
				AND n_nn.network_id = dr.network_id
	),
	nsa(
		network_id,
		substitution_category_id,
		substitute_network_id,
		weight,
		start_date,
		end_date
	) AS (
		SELECT
			network_id,
			substitution_category_id,
			substitute_network_id,
			weight,
			effective_date start_date,
			'12/31/2032' end_date
		FROM
			network_substitutions ns
	
		UNION
	
		SELECT
			network_id,
			substitution_category_id,
			substitute_network_id,
			weight,
			start_date,
			end_date
		FROM
			network_substitution_histories ns
	),
	nnsa(
			nielsen_network_id,
			substitution_category,
			substitute_nielsen_network_id,
			weight,
			start_date,
			end_date
	) AS (
		SELECT
			net_m.nielsen_network_id nielsen_network_id,
			sc.name substitution_category,
			sub_m.nielsen_network_id substitute_nielsen_network_id,
			ns.weight,
			ns.start_date,
			ns.end_date
		FROM
			nsa ns
			JOIN dbo.substitution_categories sc
				ON sc.id = ns.substitution_category_id
			JOIN n_nn net_m
				ON ns.network_id = net_m.network_id
			JOIN n_nn sub_m
				ON ns.substitute_network_id = sub_m.network_id
	),
	u_ah(
		base_media_month_id,
		forecASt_media_month_id,
		nielsen_network_id, 
		audience_id, 
		universe
	) AS (
		SELECT 
			u.base_media_month_id,
			u.forecast_media_month_id,
			u.nielsen_network_id, 
			u.audience_id, 
			(sum(u.universe * rbmm.weeks) / (SELECT sum(weeks) FROM rbmm)) * 0.001 [universe]
		FROM 
			universes u
			JOIN rbmm
				ON rbmm.media_month_id = u.forecast_media_month_id
				AND rbmm.media_month_id = u.base_media_month_id
			LEFT JOIN nnsa
				ON nnsa.nielsen_network_id = u.nielsen_network_id
				AND 'Universe' = nnsa.substitution_category
				AND rbmm.start_date between nnsa.start_date AND nnsa.end_date
		WHERE
			u.audience_id = @hhAudienceID
			AND
			nnsa.nielsen_network_id IS NULL
			AND
			u.rating_category_id = @ratingCategoryId
		GROUP BY 
			u.base_media_month_id,
			u.forecast_media_month_id,
			u.nielsen_network_id,
			u.audience_id
	
		union
	
		SELECT 
			u.base_media_month_id,
			u.forecast_media_month_id,
			nnsa.nielsen_network_id, 
			u.audience_id, 
			(sum(u.universe * rbmm.weeks) / (SELECT sum(weeks) FROM rbmm)) * 
				max(nnsa.weight) *
				0.001 [universe]
		FROM 
			universes u
			JOIN rbmm
				ON rbmm.media_month_id = u.forecast_media_month_id
				AND rbmm.media_month_id = u.base_media_month_id
			JOIN nnsa
				ON nnsa.substitute_nielsen_network_id = u.nielsen_network_id
				AND 'Universe' = nnsa.substitution_category
				AND rbmm.start_date between nnsa.start_date AND nnsa.end_date
		WHERE
			u.audience_id = @hhAudienceID
			AND u.rating_category_id = @ratingCategoryId
		GROUP BY 
			u.base_media_month_id,
			u.forecast_media_month_id,
			nnsa.nielsen_network_id, 
			u.audience_id
	),
	r_ah(
		base_media_month_id,
		forecast_media_month_id,
		nielsen_network_id, 
		audience_id, 
		daypart_id, 
		audience_usage
	) AS (
		SELECT 
			r.base_media_month_id,
			r.forecast_media_month_id,
			r.nielsen_network_id, 
			r.audience_id, 
			fmdp.daypart_id, 
			(sum(r.audience_usage * rbmm.weeks) / (SELECT sum(weeks) FROM rbmm)) * 0.001 [delivery]
		FROM 
			ratings r
			JOIN rbmm
				ON rbmm.media_month_id = r.forecast_media_month_id
				AND rbmm.media_month_id = r.base_media_month_id
			JOIN fmdp
				ON fmdp.daypart_id = r.daypart_id
			LEFT JOIN nnsa
				ON nnsa.nielsen_network_id = r.nielsen_network_id
				AND 'Delivery' = nnsa.substitution_category
				AND rbmm.start_date between nnsa.start_date AND nnsa.end_date
		WHERE
			nnsa.nielsen_network_id IS NULL
			AND r.audience_id = @hhAudienceID
			AND r.rating_category_id = @ratingCategoryId
		GROUP BY 
			r.base_media_month_id,
			r.forecast_media_month_id,
			r.nielsen_network_id, 
			fmdp.daypart_id,
			r.audience_id
	
		UNION
	
		SELECT 
			r.base_media_month_id,
			r.forecast_media_month_id,
			nnsa.nielsen_network_id, 
			r.audience_id, 
			r.daypart_id, 
			(sum(r.audience_usage * rbmm.weeks * nnsa.weight) / (SELECT sum(weeks) FROM rbmm)) * 0.001 [delivery]
		FROM 
			ratings r
			JOIN rbmm
				ON rbmm.media_month_id = r.forecast_media_month_id
				AND rbmm.media_month_id = r.base_media_month_id
			JOIN nnsa
				ON nnsa.substitute_nielsen_network_id = r.nielsen_network_id
				AND 'Delivery' = nnsa.substitution_category
				AND rbmm.start_date between nnsa.start_date AND nnsa.end_date
			JOIN fmdp
				ON fmdp.daypart_id = r.daypart_id
		WHERE
			r.audience_id = @hhAudienceID
			AND r.rating_category_id = @ratingCategoryId
		GROUP BY 
			r.base_media_month_id,
			r.forecast_media_month_id,
			nnsa.nielsen_network_id, 
			r.daypart_id,
			r.audience_id
	),
	new_rb (
		media_month_id,
		daypart_id,
		nielsen_network_id,
		audience_id,
		spot_length_id,
		delivered_rating,
		actual_usage,
		actual_universe,
		actual_rating,
		bias
	) AS (
		SELECT
			nn_dr.media_month_id,
			r.daypart_id,
			nn_dr.nielsen_network_id,
			r.audience_id,
			sl.id spot_length_id,
			nn_dr.rating * 0.01 delivered_rating,
			r.audience_usage actual_usage,
			u.universe actual_universe,
			r.audience_usage / u.universe actual_rating,
			(nn_dr.rating * 0.01) / (r.audience_usage / u.universe) bias
		FROM
			nn_dr
			JOIN r_ah r
				ON nn_dr.media_month_id = r.base_media_month_id
				AND nn_dr.media_month_id = r.forecast_media_month_id
				AND nn_dr.nielsen_network_id = r.nielsen_network_id
			JOIN u_ah u
				ON nn_dr.media_month_id = u.base_media_month_id
				AND nn_dr.media_month_id = u.forecast_media_month_id
				AND nn_dr.nielsen_network_id = u.nielsen_network_id,
			spot_lengths sl
	),
	avg_rb(
		media_month_id,
		daypart_id,
		nielsen_network_id,
		audience_id,
		spot_length_id,
		bias
	) AS (
		SELECT
			@baseMonthID media_month_id,
			new_rb.daypart_id,
			new_rb.nielsen_network_id,
			new_rb.audience_id,
			new_rb.spot_length_id,
			avg(new_rb.bias) bias
		FROM
			new_rb
			JOIN media_months mm
				ON mm.id = new_rb.media_month_id
			JOIN nielsen_networks nn
				ON nn.id = new_rb.nielsen_network_id
			JOIN spot_lengths sl
				ON sl.id = new_rb.spot_length_id
		WHERE
			bias > 0.5
		GROUP BY
			new_rb.daypart_id,
			new_rb.nielsen_network_id,
			new_rb.audience_id,
			new_rb.spot_length_id
	)
	
	insert into
		rotational_biases(
			media_month_id,
			daypart_id,
			nielsen_network_id,
			audience_id,
			spot_length_id,
			bias,
			rating_category_id
		)
	SELECT
		avg_rb.media_month_id,
		avg_rb.daypart_id,
		avg_rb.nielsen_network_id,
		avg_rb.audience_id,
		avg_rb.spot_length_id,
	--	cast(mm.month AS varchar) + '/' + cASt(mm.year AS varchar) [Month],
	--	dp.daypart_text [Daypart],
	--	nn.code [Network],
	--	dbo.GetAudienceStringFROMID(avg_rb.audience_id) [Audience],
	--	sl.length [Spot Length],
		CASE 
			WHEN avg_rb.bias <= 0.5 THEN 1
			WHEN avg_rb.bias > 1.5 THEN 1
			ELSE avg_rb.bias 
		END [Bias],
		@ratingCategoryId
	FROM
		avg_rb
		JOIN media_months mm
			ON mm.id = avg_rb.media_month_id
		JOIN nielsen_networks nn
			ON nn.id = avg_rb.nielsen_network_id
		JOIN spot_lengths sl
			ON sl.id = avg_rb.spot_length_id
		JOIN dayparts dp
			ON dp.id = avg_rb.daypart_id
	WHERE
	--	sl.length in (15, 30, 60)
		sl.length = 30
	ORDER BY
		media_month_id,
		daypart_id,
		nielsen_network_id,
		audience_id,
		spot_length_id;
END

