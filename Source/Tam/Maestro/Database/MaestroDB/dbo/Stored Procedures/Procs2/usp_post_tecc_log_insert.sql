-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/25/2014 09:19:01 AM
-- Description:	Auto-generated method to insert a post_tecc_log record.
-- =============================================
CREATE PROCEDURE usp_post_tecc_log_insert
	@tam_post_id INT,
	@result_status INT,
	@employee_id INT,
	@transmitted_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[post_tecc_log]
	(
		[tam_post_id],
		[result_status],
		[employee_id],
		[transmitted_date]
	)
	VALUES
	(
		@tam_post_id,
		@result_status,
		@employee_id,
		@transmitted_date
	)
END

