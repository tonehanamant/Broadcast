
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_TCS_GetProposalInformationByTrafficOrder]
	@id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		proposals.id,
		(SELECT TOP 1 employee_id FROM proposal_employees (NOLOCK) WHERE proposal_id=traffic_proposals.proposal_id ORDER BY effective_date DESC) 'creator_employee_id',
		proposals.salesperson_employee_id,
		ISNULL(proposals.agency_company_id, 0),
		ISNULL(products.company_id, 0),
		ISNULL(proposals.product_id, 0),
		(SELECT TOP 1 contact_id FROM proposal_contacts (NOLOCK) WHERE proposal_id=traffic_proposals.proposal_id ORDER BY date_created DESC) 'contact_id',
		proposals.name 'title',
		(SELECT TOP 1 employees.firstname + ' ' + employees.lastname FROM proposal_employees (NOLOCK) JOIN employees (NOLOCK) ON employees.id=proposal_employees.employee_id WHERE proposal_employees.proposal_id=traffic_proposals.proposal_id ORDER BY proposal_employees.effective_date DESC) 'creator_name',
		(employees.firstname + ' ' + employees.lastname) 'salesperson_name',
		'Not applicable' 'rate_card_description',
		'Gross' 'billing_type',
		proposals.advertiser_company_id 'advertiser_id',
		dbo.GetProductsForMarriedProposal(proposals.id) 'product_name',
		proposals.agency_company_id 'agency_id',
		(SELECT TOP 1 contacts.first_name + ' ' + contacts.last_name FROM proposal_contacts (NOLOCK) JOIN contacts (NOLOCK) ON contacts.id=proposal_contacts.contact_id WHERE proposal_contacts.proposal_id=traffic_proposals.proposal_id	ORDER BY proposal_contacts.date_created DESC) 'buyer_name',
		proposals.start_date,
		proposals.end_date,  
		proposals.traffic_notes, 
		traffic.status_id, 
		proposals.flight_text, 
		vw_ccc_daypart.id
	FROM proposals (NOLOCK) 
		LEFT JOIN employees (NOLOCK) ON employees.id=proposals.salesperson_employee_id 
		LEFT JOIN products (NOLOCK) ON products.id=proposals.product_id 
		INNER JOIN traffic_proposals (NOLOCK) on traffic_proposals.proposal_id = proposals.id
		INNER JOIN traffic (NOLOCK) on traffic.id = traffic_proposals.traffic_id
INNER JOIN vw_ccc_daypart (NOLOCK) on vw_ccc_daypart.id = proposals.default_daypart_id
	WHERE 
		traffic.id=@id 
	AND traffic_proposals.primary_proposal = 1
END

