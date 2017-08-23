-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/25/2014 09:19:02 AM
-- Description:	Auto-generated method to delete a single post_tecc_log record.
-- =============================================
CREATE PROCEDURE usp_post_tecc_log_delete
	@tam_post_id INT,
	@employee_id INT,
	@transmitted_date DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[post_tecc_log]
	WHERE
		[tam_post_id]=@tam_post_id
		AND [employee_id]=@employee_id
		AND [transmitted_date]=@transmitted_date
END

