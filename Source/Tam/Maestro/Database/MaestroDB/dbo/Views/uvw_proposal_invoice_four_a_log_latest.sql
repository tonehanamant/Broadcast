CREATE VIEW uvw_proposal_invoice_four_a_log_latest AS 

	SELECT proposal_id, receivable_invoice_id, employee_id, emp.firstname, emp.lastname, exported_date
	FROM (SELECT proposal_id, receivable_invoice_id, employee_id, exported_date,
			RANK() OVER (PARTITION BY proposal_id, receivable_invoice_id ORDER BY exported_date DESC) uploaded_rank
			FROM proposal_invoice_four_a_log (NOLOCK)) AS history_rankings
	INNER JOIN employees emp (NOLOCK) ON history_rankings.employee_id = emp.id
	WHERE uploaded_rank = 1

