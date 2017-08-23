
CREATE PROCEDURE [dbo].[usp_PCS_ExportMediaPlanByDemos]
	@proposal_id INT,
	@audience_id INT,
	@audience_id_2 INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		products.name,
		proposals.advertiser_company_id,
		proposals.agency_company_id,
		proposals.name,
		spot_lengths.length,
		proposals.default_daypart_id,
		proposals.flight_text,
		proposals.date_created,
		dbo.GetProposalVersionIdentifier(proposals.id) 'version_identifier', 
		proposals.is_equivalized,
		proposals.rate_card_type_id,
		proposals.proposal_status_id,
		proposals.guarantee_type,
		proposals.start_date,
		proposals.end_date,
		proposals.base_ratings_media_month_id,
		proposals.base_universe_media_month_id,
		proposals.rating_source_id,
		proposals.buyer_note,
		proposals.print_title,
		dbo.GetProposalSalesModel(proposals.id) 'sales_model_id',
		proposals.start_date,
		proposals.end_date,
		proposals.agency_company_id
	FROM 
		proposals (NOLOCK)
		LEFT JOIN products (NOLOCK) ON products.id=proposals.product_id 
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=proposals.default_spot_length_id
	WHERE 
		proposals.id=@proposal_id


	SELECT 
		pa.audience_id,
		pa.universe 
	FROM 
		proposal_audiences pa (NOLOCK) 
	WHERE 
		pa.proposal_id=@proposal_id 
		AND pa.audience_id IN (31,@audience_id,@audience_id_2)
	ORDER BY 
		pa.ordinal


	SELECT 
		a.id,
		a.code,
		a.name
	FROM 
		audiences a (NOLOCK) 
	WHERE 
		a.id IN (@audience_id,@audience_id_2)


	SELECT 
		pd.id,
		pd.topography_universe,
		pd.proposal_rate,
		n.code,
		pd.num_spots,
		pd.include,
		pd.daypart_id,
		pd.network_id,
		sl.id,
		sl.length,
		sl.delivery_multiplier,
		sl.order_by,
		sl.is_default,
		pd.daypart_id
	FROM
		proposal_details pd (NOLOCK) 
		JOIN proposals p (NOLOCK) ON p.id=pd.proposal_id
		JOIN uvw_network_universe n	(NOLOCK) ON n.network_id=pd.network_id 
			AND (n.start_date<=p.start_date AND (n.end_date>=p.start_date OR n.end_date IS NULL))
		JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
	WHERE 
		pd.proposal_id=@proposal_id 
	ORDER BY 
		n.code


	SELECT 
		pd.id,
		pda.audience_id,
		pda.rating,
		pda.us_universe 
	FROM 
		proposal_detail_audiences pda (NOLOCK) 
		JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id
	WHERE 
		pd.proposal_id=@proposal_id 
		AND pda.audience_id IN (31,@audience_id,@audience_id_2)


	SELECT
		pdw.proposal_detail_id,
		pdw.media_week_id,
		pdw.units 'units'
	FROM
		proposal_detail_worksheets pdw (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
		JOIN proposal_details pd (NOLOCK) ON pd.id=pdw.proposal_detail_id
	WHERE
		pd.proposal_id=@proposal_id
		

	SELECT
		mm.id,
		mm.media_month,
		SUM(CAST(pdw.units AS MONEY) * pd.proposal_rate) 'gross_due',
		SUM(pdw.units) 'units'
	FROM
		proposal_detail_worksheets pdw (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
		JOIN proposal_details pd (NOLOCK) ON pd.id=pdw.proposal_detail_id
	WHERE
		pd.proposal_id=@proposal_id
		AND mm.id IN (
			SELECT DISTINCT 
				mw.media_month_id 
			FROM 
				dbo.proposal_flights pf (NOLOCK) 
				JOIN dbo.media_weeks mw (NOLOCK) ON mw.start_date <= pf.end_date AND mw.end_date >= pf.start_date 
			WHERE
				pf.proposal_id=@proposal_id
				AND pf.selected=1
			)
	GROUP BY
		mm.id,
		mm.media_month
		
		
	SELECT
		mw.id,
		mw.week_number,
		mw.media_month_id,
		mm.year,
		mm.month,
		mw.start_date,
		mw.end_date,
		mm.start_date,
		mm.end_date
	FROM
		dbo.proposal_flights pf (NOLOCK)
		JOIN dbo.media_weeks mw (NOLOCK) ON mw.start_date <= pf.end_date AND mw.end_date >= pf.start_date
		JOIN dbo.media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		pf.proposal_id=@proposal_id
		AND pf.selected=1
	ORDER BY
		mw.start_date
		
		
	SELECT
		mm.id,
		MIN(pf.start_date) 'start_date',
		MAX(pf.end_date) 'end_date'
	FROM
		dbo.proposal_flights pf (NOLOCK)
		JOIN dbo.media_months mm (NOLOCK) ON mm.start_date <= pf.end_date AND mm.end_date >= pf.start_date
	WHERE
		pf.proposal_id=@proposal_id
		AND pf.selected=1
	GROUP BY
		mm.id
END
