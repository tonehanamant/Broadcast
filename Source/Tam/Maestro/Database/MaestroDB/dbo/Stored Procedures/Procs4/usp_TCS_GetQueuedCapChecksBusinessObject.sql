-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_TCS_GetQueuedCapChecksBusinessObject]
AS
BEGIN
	SET NOCOUNT ON;

	-- list of proposals for both monthly/cumulative traffic cap queues
	SELECT
		dp.*
	FROM
		uvw_display_proposals dp
	WHERE
		dp.id IN (
			SELECT DISTINCT tp.proposal_id FROM traffic_monthly_cap_queues q (NOLOCK) JOIN traffic_proposals tp (NOLOCK) ON tp.traffic_id=q.traffic_id
			UNION
			SELECT DISTINCT q.proposal_id FROM traffic_cumulative_cap_queues q (NOLOCK)
		)


	-- monthly traffic cap queue
	SELECT
		q.date_created,
		e.firstname + ' ' + e.lastname 'employee_name',
		o.traffic_id,
		o.traffic_name,
		o.proposal_id,
		o.proposal_title,
		o.contract_dollars,
		o.contract_cap_dollars,
		o.release_dollars,
		o.release_overage_dollars,
		o.release_overage_status,
		o.is_release_over,
		o.release_is_overruled,
		r.name 'release_name'
	FROM
		traffic_monthly_cap_queues q (NOLOCK)
		JOIN employees e (NOLOCK) ON e.id=q.employee_id
		JOIN traffic_proposals tp (NOLOCK) ON tp.traffic_id=q.traffic_id
		CROSS APPLY dbo.udf_GetMonthlyTrafficCapOverage(q.traffic_id) o
		JOIN traffic t (NOLOCK) ON t.id=o.traffic_id
		JOIN releases r (NOLOCK) ON r.id=t.release_id
	ORDER BY
		q.date_created DESC


	-- cumulative 
	SELECT
		q.date_created,
		e.firstname + ' ' + e.lastname 'employee_name',
		o.proposal_id,
		o.proposal_title,
		o.contract_dollars,
		o.contract_cap_dollars,
		o.release_dollars,
		o.release_overage_dollars,
		o.release_overage_status,
		o.is_release_over,
		o.release_is_overruled
	FROM
		traffic_cumulative_cap_queues q (NOLOCK)
		JOIN employees e (NOLOCK) ON e.id=q.employee_id
		CROSS APPLY dbo.udf_GetCumulativeTrafficCapOverageByProposal(q.proposal_id) o
		JOIN proposals p (NOLOCK) ON p.id=q.proposal_id
	ORDER BY
		q.date_created DESC

	
	-- get monthly override approval history
	SELECT
		e.firstname + ' ' + e.lastname 'employee_name',
		tcmoa.*
	FROM
		traffic_cap_monthly_override_approvals tcmoa (NOLOCK)
		JOIN employees e (NOLOCK) ON e.id=tcmoa.employee_id
	WHERE
		tcmoa.traffic_id IN (
			SELECT q.traffic_id FROM traffic_monthly_cap_queues q (NOLOCK)
		)
	ORDER BY
		tcmoa.traffic_id,
		tcmoa.approval_date DESC


	-- get cumulative override approval history
	SELECT
		e.firstname + ' ' + e.lastname 'employee_name',
		tccoa.*
	FROM
		traffic_cap_cumulative_override_approvals tccoa (NOLOCK)
		JOIN employees e (NOLOCK) ON e.id=tccoa.employee_id
	WHERE
		tccoa.proposal_id IN (
			SELECT q.proposal_id FROM traffic_cumulative_cap_queues q (NOLOCK)
		)
	ORDER BY
		tccoa.proposal_id,
		tccoa.approval_date DESC
END
