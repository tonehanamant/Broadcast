CREATE VIEW [dbo].[uvw_post_tecc_log_latest] AS 

SELECT tam_post_id, result_status, employee_id, emp.firstname, emp.lastname, transmitted_date
FROM (SELECT tam_post_id, result_status, employee_id, transmitted_date,
		RANK() OVER (PARTITION BY tam_post_id ORDER BY transmitted_date DESC) uploaded_rank
		FROM post_tecc_log (NOLOCK)) AS log_rankings
INNER JOIN employees emp (NOLOCK) ON log_rankings.employee_id = emp.id
WHERE uploaded_rank = 1
	
