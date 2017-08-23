-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/04/2014 01:01:55 PM
-- Description:	Auto-generated method to delete a single proposal_tecc_log record.
-- =============================================
CREATE PROCEDURE usp_proposal_tecc_log_delete
	@proposal_id INT,
	@employee_id INT,
	@transmitted_date DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[proposal_tecc_log]
	WHERE
		[proposal_id]=@proposal_id
		AND [employee_id]=@employee_id
		AND [transmitted_date]=@transmitted_date
END

