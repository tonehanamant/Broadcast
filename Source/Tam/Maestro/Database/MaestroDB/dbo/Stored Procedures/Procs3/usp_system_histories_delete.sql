
-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/11/2017 11:10:52 AM
-- Description:	Auto-generated method to delete a single system_histories record.
-- =============================================
CREATE PROCEDURE usp_system_histories_delete
	@system_id INT,
	@start_date DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[system_histories]
	WHERE
		[system_id]=@system_id
		AND [start_date]=@start_date
END
