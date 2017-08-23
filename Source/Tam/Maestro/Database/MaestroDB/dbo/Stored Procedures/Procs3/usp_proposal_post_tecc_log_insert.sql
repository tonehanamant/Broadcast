-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/25/2014 09:15:33 AM
-- Description:	Auto-generated method to insert a proposal_post_tecc_log record.
-- =============================================
CREATE PROCEDURE usp_proposal_post_tecc_log_insert
	@proposal_id INT,
	@tam_post_id INT,
	@employee_id INT,
	@transmitted_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[proposal_post_tecc_log]
	(
		[proposal_id],
		[tam_post_id],
		[employee_id],
		[transmitted_date]
	)
	VALUES
	(
		@proposal_id,
		@tam_post_id,
		@employee_id,
		@transmitted_date
	)
END

