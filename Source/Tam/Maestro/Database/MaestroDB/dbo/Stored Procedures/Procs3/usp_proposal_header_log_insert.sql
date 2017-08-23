-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/31/2014 09:41:33 AM
-- Description:	Auto-generated method to insert a proposal_header_log record.
-- =============================================
CREATE PROCEDURE usp_proposal_header_log_insert
	@proposal_id INT,
	@employee_id INT,
	@uploaded_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[proposal_header_log]
	(
		[proposal_id],
		[employee_id],
		[uploaded_date]
	)
	VALUES
	(
		@proposal_id,
		@employee_id,
		@uploaded_date
	)
END
