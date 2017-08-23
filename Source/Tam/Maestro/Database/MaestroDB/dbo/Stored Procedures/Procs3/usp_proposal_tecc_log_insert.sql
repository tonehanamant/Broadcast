-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/04/2014 01:01:55 PM
-- Description:	Auto-generated method to insert a proposal_tecc_log record.
-- =============================================
CREATE PROCEDURE usp_proposal_tecc_log_insert
	@proposal_id INT,
	@employee_id INT,
	@transmitted_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[proposal_tecc_log]
	(
		[proposal_id],
		[employee_id],
		[transmitted_date]
	)
	VALUES
	(
		@proposal_id,
		@employee_id,
		@transmitted_date
	)
END

