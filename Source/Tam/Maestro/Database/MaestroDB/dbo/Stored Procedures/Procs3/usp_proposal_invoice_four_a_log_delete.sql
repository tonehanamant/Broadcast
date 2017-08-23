-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/07/2014 04:11:44 PM
-- Description:	Auto-generated method to delete a single proposal_invoice_four_a_log record.
-- =============================================
CREATE PROCEDURE usp_proposal_invoice_four_a_log_delete
	@proposal_id INT,
	@receivable_invoice_id INT,
	@employee_id INT,
	@exported_date DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[proposal_invoice_four_a_log]
	WHERE
		[proposal_id]=@proposal_id
		AND [receivable_invoice_id]=@receivable_invoice_id
		AND [employee_id]=@employee_id
		AND [exported_date]=@exported_date
END

