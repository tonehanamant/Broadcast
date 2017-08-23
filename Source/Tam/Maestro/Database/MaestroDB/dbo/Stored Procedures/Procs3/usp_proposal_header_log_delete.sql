-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/31/2014 09:41:34 AM
-- Description:	Auto-generated method to delete a single proposal_header_log record.
-- =============================================
CREATE PROCEDURE usp_proposal_header_log_delete
	@proposal_id INT,
	@employee_id INT,
	@uploaded_date DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[proposal_header_log]
	WHERE
		[proposal_id]=@proposal_id
		AND [employee_id]=@employee_id
		AND [uploaded_date]=@uploaded_date
END

