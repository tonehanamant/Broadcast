-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/25/2014 09:19:01 AM
-- Description:	Auto-generated method to select all post_tecc_log records.
-- =============================================
CREATE PROCEDURE usp_post_tecc_log_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[post_tecc_log].*
	FROM
		[dbo].[post_tecc_log] WITH(NOLOCK)
END

