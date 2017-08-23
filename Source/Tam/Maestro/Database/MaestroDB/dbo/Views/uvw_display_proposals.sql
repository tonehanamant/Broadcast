
/* BEGIN MME-1306 - Missing IsEquivalized column in uvw_display_proposal*/ 

CREATE VIEW [dbo].[uvw_display_proposals]
AS
SELECT        
p.id, 
p.original_proposal_id,
p.version_number, 
ps.name AS status, 
p.total_gross_cost, 
adv.name AS advertiser, 
CASE WHEN pr.name IS NULL THEN dbo.GetProductForProposal(p.id) ELSE pr.name END AS product, 
agy.name AS agency, 
p.name AS title, 
e.firstname + ' ' + e.lastname AS salesperson, 
p.flight_text, 
a.name AS primary_audience, 
p.include_on_availability_planner, 
p.date_created, p.date_last_modified, 
p.proposal_status_id, 
ISNULL(p.is_audience_deficiency_unit_schedule, CAST(0 AS BIT)) AS is_audience_deficiency_unit_schedule, 
p.rating_source_id, 
p.start_date, 
p.end_date, 
p.rate_card_type_id, 
p.total_spots, 
d.daypart_text AS primary_daypart, 
CASE WHEN pp.proposal_id IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS has_been_posted,
po.proposal_status_id AS original_proposal_status_id, 
p.number_of_materials, 
mm.media_month, 
psm.sales_model_id, 
CASE WHEN p.is_msa IS NULL THEN CAST(0 AS BIT) ELSE p.is_msa END AS is_msa,
CASE WHEN ftp.proposal_id IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS has_been_fast_tracked, 
p.posting_media_month_id, 
sl.length, a.id AS primary_audience_id, 
p.product_id, 
a.code AS primary_audience_code, 
CASE WHEN msa.proposal_id IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS has_been_msa_posted, 
p.is_media_ocean_plan, 
CAST(CASE WHEN ptll.proposal_id IS NULL THEN 0 ELSE 1 END AS BIT) AS is_media_ocean_tecc_file_sent, 
dbo.ufn_generate_audit_string(ptll.firstname, ptll.lastname, ptll.transmitted_date) AS media_ocean_last_sent,
p.audience_deficiency_unit_for,
p.is_overnight, 
c.name AS cluster_name, 
p.is_advanced_tv, 
p.is_equivalized
FROM            dbo.proposals AS p WITH (NOLOCK) LEFT OUTER JOIN
                         dbo.products AS pr WITH (NOLOCK) ON pr.id = p.product_id LEFT OUTER JOIN
                         dbo.companies AS adv WITH (NOLOCK) ON adv.id = p.advertiser_company_id LEFT OUTER JOIN
                         dbo.companies AS agy WITH (NOLOCK) ON agy.id = p.agency_company_id LEFT OUTER JOIN
                         dbo.employees AS e WITH (NOLOCK) ON e.id = p.salesperson_employee_id LEFT OUTER JOIN
                         dbo.proposal_statuses AS ps WITH (NOLOCK) ON ps.id = p.proposal_status_id LEFT OUTER JOIN
                         dbo.proposal_audiences AS pa WITH (NOLOCK) ON pa.proposal_id = p.id AND pa.ordinal = 1 LEFT OUTER JOIN
                         dbo.audiences AS a WITH (NOLOCK) ON a.id = pa.audience_id LEFT OUTER JOIN
                         dbo.dayparts AS d WITH (NOLOCK) ON d.id = p.primary_daypart_id LEFT OUTER JOIN
                         dbo.proposals AS po WITH (NOLOCK) ON po.id = p.original_proposal_id LEFT OUTER JOIN
                         dbo.media_months AS mm WITH (NOLOCK) ON mm.id = p.posting_media_month_id LEFT OUTER JOIN
                         dbo.uvw_posted_proposals AS pp ON pp.proposal_id = p.id LEFT OUTER JOIN
                         dbo.uvw_fast_tracked_proposals AS ftp ON ftp.proposal_id = p.id LEFT OUTER JOIN
                         dbo.uvw_msa_posted_proposals AS msa ON msa.proposal_id = p.id LEFT OUTER JOIN
                         dbo.proposal_sales_models AS psm WITH (NOLOCK) ON psm.proposal_id = p.id LEFT OUTER JOIN
                         dbo.spot_lengths AS sl WITH (NOLOCK) ON sl.id = p.default_spot_length_id LEFT OUTER JOIN
                         dbo.uvw_proposal_tecc_log_latest AS ptll WITH (NOLOCK) ON p.id = ptll.proposal_id LEFT OUTER JOIN
                         dbo.categories AS c WITH (NOLOCK) ON p.category_id = c.id 
