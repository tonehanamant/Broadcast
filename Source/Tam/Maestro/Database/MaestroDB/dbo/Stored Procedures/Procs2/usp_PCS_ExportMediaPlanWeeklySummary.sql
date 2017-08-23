
CREATE PROCEDURE [dbo].[usp_PCS_ExportMediaPlanWeeklySummary]
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- proposal info
	SELECT 
		proposals.agency_company_id,
		proposals.advertiser_company_id,
		products.name,		
		proposals.flight_text,
		dbo.GetProposalVersionIdentifier(proposals.id) 'version_identifier',
		proposals.buyer_note,
		proposals.print_title
	FROM 
		proposals						(NOLOCK) 
		LEFT JOIN products				(NOLOCK) ON products.id=proposals.product_id 
	WHERE 
		proposals.id=@proposal_id

	-- display media weeks
	SELECT
		mw.id,
		mw.week_number,
		mm.id,
		mm.year,
		mm.month,
		mw.start_date,
		mw.end_date,
		mm.start_date,
		mm.end_date
	FROM
		media_weeks mw (NOLOCK) 
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		mw.id IN (
			SELECT media_week_id FROM proposal_detail_worksheets WHERE proposal_detail_id IN (
				SELECT id FROM proposal_details WHERE proposal_id=@proposal_id
			)
		)
	ORDER BY
		mw.start_date ASC

	-- media months
	SELECT DISTINCT
		mm.id,
		mm.year,
		mm.month,
		mm.media_month,
		mm.start_date,
		mm.end_date
	FROM
		media_weeks mw (NOLOCK) 
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		mw.id IN (
			SELECT media_week_id FROM proposal_detail_worksheets (NOLOCK) WHERE proposal_detail_id IN (
				SELECT id FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id
			)
		)
	ORDER BY
		mm.start_date ASC

	-- proposal details
    SELECT
		pd.id,
		n.code,
		d.daypart_text,
		sl.length,
		pd.num_spots,
		pd.proposal_rate
	FROM
		proposal_details pd				(NOLOCK)
		JOIN proposals p				(NOLOCK) ON p.id=pd.proposal_id
		JOIN uvw_network_universe n		(NOLOCK) ON n.network_id=pd.network_id AND (n.start_date<=p.start_date AND (n.end_date>=p.start_date OR n.end_date IS NULL))
		JOIN dayparts d					(NOLOCK) ON d.id=pd.daypart_id
		JOIN spot_lengths sl			(NOLOCK) ON sl.id=pd.spot_length_id
	WHERE
		pd.proposal_id=@proposal_id
	ORDER BY
		d.daypart_text,
		n.code,
		sl.length

	-- worksheet data
	SELECT
		pdw.proposal_detail_id,
		pdw.units,
		pdw.media_week_id
	FROM
		proposal_detail_worksheets	pdw		(NOLOCK)
		JOIN media_weeks mw					(NOLOCK) ON mw.id=pdw.media_week_id
		JOIN proposal_details pd			(NOLOCK) ON pd.id=pdw.proposal_detail_id
	WHERE
		pd.proposal_id=@proposal_id
	ORDER BY
		mw.start_date ASC
END

