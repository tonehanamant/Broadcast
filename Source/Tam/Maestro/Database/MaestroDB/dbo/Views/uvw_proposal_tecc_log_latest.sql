CREATE VIEW [dbo].[uvw_proposal_tecc_log_latest] AS 

SELECT proposal_id, employee_id, emp.firstname, emp.lastname, transmitted_date
FROM (SELECT proposal_id, employee_id, transmitted_date,
		RANK() OVER (PARTITION BY proposal_id ORDER BY transmitted_date DESC) uploaded_rank
		FROM proposal_tecc_log (NOLOCK)) AS log_rankings
INNER JOIN employees emp (NOLOCK) ON log_rankings.employee_id = emp.id
WHERE uploaded_rank = 1
	
