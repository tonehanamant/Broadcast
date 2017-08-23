/****** Object:  Table [dbo].[usp_TCS_GetProposalsByIDWithoutTrafficNoFilter]    Script Date: 11/16/2012 14:51:25 ******/
CREATE PROCEDURE [dbo].[usp_TCS_GetProposalsByIDWithoutTrafficNoFilter]
	@proposal_id INT
AS
BEGIN
	SELECT
		dp.id,
		dp.version_number,
		dp.total_gross_cost,
		dp.advertiser,
		dp.product,
		dp.agency,
		dp.title,
		dp.salesperson,
		dp.flight_text,
		dp.include_on_availability_planner,
		dp.date_created,
		dp.date_last_modified,
		ps.name 'proposal_status',
		dp.length,
		rct.name 'rate_card_type',
		ISNULL(dp.original_proposal_id, dp.id)
	FROM
		uvw_display_proposals dp
		JOIN proposal_statuses ps (NOLOCK) ON ps.id=dp.proposal_status_id
		JOIN rate_card_types rct (NOLOCK) ON rct.id=dp.rate_card_type_id
	WHERE
		dp.id=@proposal_id 
		AND dp.sales_model_id <> 4 
		AND dp.proposal_status_id NOT IN (5,6,7) -- previously ordered, cancelled before start, posting plan
END
