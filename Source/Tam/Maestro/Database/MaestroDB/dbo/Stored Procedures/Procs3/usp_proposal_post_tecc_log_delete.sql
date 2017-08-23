﻿-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/25/2014 09:15:34 AM
-- Description:	Auto-generated method to delete a single proposal_post_tecc_log record.
-- =============================================
CREATE PROCEDURE usp_proposal_post_tecc_log_delete
	@proposal_id INT,
	@tam_post_id INT,
	@employee_id INT,
	@transmitted_date DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[proposal_post_tecc_log]
	WHERE
		[proposal_id]=@proposal_id
		AND [tam_post_id]=@tam_post_id
		AND [employee_id]=@employee_id
		AND [transmitted_date]=@transmitted_date
END

