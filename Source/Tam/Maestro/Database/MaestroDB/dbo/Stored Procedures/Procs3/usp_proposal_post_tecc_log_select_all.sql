-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/25/2014 09:15:33 AM
-- Description:	Auto-generated method to select all proposal_post_tecc_log records.
-- =============================================
CREATE PROCEDURE usp_proposal_post_tecc_log_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_post_tecc_log].*
	FROM
		[dbo].[proposal_post_tecc_log] WITH(NOLOCK)
END

