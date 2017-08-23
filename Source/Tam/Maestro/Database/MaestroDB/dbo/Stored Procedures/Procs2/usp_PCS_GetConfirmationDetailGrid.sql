
CREATE PROCEDURE [dbo].[usp_PCS_GetConfirmationDetailGrid]
	@year INT,
	@quarter INT,
	@month INT
AS
BEGIN
	CREATE TABLE #proposals (proposal_id INT)
	INSERT INTO #proposals
		SELECT DISTINCT 
			proposal_id
		FROM 
			proposal_flights pf (NOLOCK) 
			JOIN media_months mm (NOLOCK) ON (mm.start_date <= pf.end_date AND mm.end_date >= pf.start_date)
			JOIN proposals p (NOLOCK) ON p.id=pf.proposal_id
		WHERE
			p.proposal_status_id=4
			AND (@year=mm.[year])
			AND (@quarter=CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END)
			AND (@month=mm.[month])

	-- display confirmation
	SELECT
		p.id,
		GETDATE() 'date',
		p.agency_company_id 'agency',
		p.advertiser_company_id 'advertiser',
		(SELECT TOP 1 c.first_name + ' ' + c.last_name FROM proposal_contacts pc (NOLOCK) JOIN contacts c (NOLOCK) ON c.id=pc.contact_id WHERE pc.proposal_id=p.id ORDER BY pc.date_created DESC) 'contact',
		sl.length,
		p.flight_text,
		bt.name 'billing_terms',
		bt.id,
		a.name,
		a.address_line_1,
		a.address_line_2,
		a.address_line_3,
		a.city,
		st.code,
		a.zip,
		(SELECT TOP 1 sales_model_id FROM proposal_sales_models WHERE proposal_id=p.id) 'sales_model_id',
		p.rate_card_type_id,
		p.confirmation_notes,
		pr.name,
		p.media_plan_format_code,
		bt.code
	FROM
		#proposals
		JOIN proposals p (NOLOCK) ON p.id=#proposals.proposal_id
		JOIN spot_lengths sl (NOLOCK) ON sl.id=p.default_spot_length_id
		LEFT JOIN addresses a (NOLOCK) ON a.id=billing_address_id
		LEFT JOIN billing_terms bt (NOLOCK) ON bt.id=p.billing_terms_id
		LEFT JOIN states st (NOLOCK) ON st.id=a.state_id
		LEFT JOIN products pr (NOLOCK) ON pr.id=p.product_id
		LEFT JOIN dayparts d (NOLOCK) ON d.id=p.primary_daypart_id
	WHERE
		p.id NOT IN (
			SELECT DISTINCT pp.parent_proposal_id FROM proposal_proposals pp (NOLOCK)
		)

	-- monthly summary
	SELECT
		pd.proposal_id,
		mm.media_month,
		mm.end_date,
		mm.[year],
		CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END 'quarter',
		mm.[month],
		SUM(pdw.units * pd.proposal_rate)
	FROM
		proposal_detail_worksheets pdw (NOLOCK)
		JOIN proposal_details pd (NOLOCK) ON pd.id=pdw.proposal_detail_id
		JOIN proposals p (NOLOCK) ON p.id=pd.proposal_id
		JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		p.id IN (
			SELECT proposal_id FROM #proposals
		)
	GROUP BY
		pd.proposal_id,
		mm.media_month,
		mm.end_date,
		mm.[year],
		mm.[month]
	ORDER BY
		pd.proposal_id,
		mm.end_date
		
	-- dayparts
	SELECT
		p.proposal_id,
		d.daypart_text
	FROM
		#proposals p
		JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=p.proposal_id
		JOIN dayparts d (NOLOCK) ON d.id=pd.daypart_id
	WHERE
		pd.num_spots>0
	GROUP BY
		p.proposal_id,
		d.daypart_text
	
	DROP TABLE #proposals;
END



