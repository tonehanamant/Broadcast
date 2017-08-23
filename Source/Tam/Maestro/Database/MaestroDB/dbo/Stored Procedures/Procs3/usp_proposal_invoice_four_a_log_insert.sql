-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/07/2014 04:11:44 PM
-- Description:	Auto-generated method to insert a proposal_invoice_four_a_log record.
-- =============================================
CREATE PROCEDURE usp_proposal_invoice_four_a_log_insert
	@proposal_id INT,
	@receivable_invoice_id INT,
	@employee_id INT,
	@exported_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[proposal_invoice_four_a_log]
	(
		[proposal_id],
		[receivable_invoice_id],
		[employee_id],
		[exported_date]
	)
	VALUES
	(
		@proposal_id,
		@receivable_invoice_id,
		@employee_id,
		@exported_date
	)
END

