CREATE PROCEDURE usp_proposal_media_ocean_logs_get
	@proposal_id INT
AS
BEGIN
	DECLARE @original_proposal_id INT
	SELECT @original_proposal_id = CASE WHEN original_proposal_id IS NULL THEN @proposal_id ELSE original_proposal_id END
	FROM proposals
	WHERE id = @proposal_id

	SELECT Uploaded.*   
	FROM (SELECT TOP 1 phh.proposal_id, 'Uploaded' AS history_type, emp.firstname, emp.lastname, phh.uploaded_date AS action_date  
			FROM proposal_header_log phh  
			INNER JOIN employees emp ON phh.employee_id = emp.id  
			WHERE phh.proposal_id = @original_proposal_id  
			ORDER BY action_date DESC) AS Uploaded  
	
	UNION ALL  
	
	SELECT TeccFileSent.*   
	FROM (SELECT TOP 1 phh.proposal_id, 'TeccFileSent' AS history_type, emp.firstname, emp.lastname, phh.transmitted_date AS action_date  
			FROM proposal_tecc_log phh  
			INNER JOIN employees emp ON phh.employee_id = emp.id  
			WHERE phh.proposal_id = @proposal_id  
			ORDER BY action_date DESC) AS TeccFileSent  
	
END

