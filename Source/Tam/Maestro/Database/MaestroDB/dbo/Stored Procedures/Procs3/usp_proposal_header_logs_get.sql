CREATE PROCEDURE usp_proposal_header_logs_get
(
	@proposal_id INT
)
AS
BEGIN

	SELECT proposal_id, employee_id, uploaded_date
	FROM proposal_header_log
	WHERE proposal_id = @proposal_id
END
