-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/25/2014 09:19:02 AM
-- Description:	Auto-generated method to update a post_tecc_log record.
-- =============================================
CREATE PROCEDURE usp_post_tecc_log_update
	@tam_post_id INT,
	@result_status INT,
	@employee_id INT,
	@transmitted_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[post_tecc_log]
	SET
		[result_status]=@result_status
	WHERE
		[tam_post_id]=@tam_post_id
		AND [employee_id]=@employee_id
		AND [transmitted_date]=@transmitted_date
END

