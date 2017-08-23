-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/04/2014 01:01:55 PM
-- Description:	Auto-generated method to delete or potentionally disable a proposal_tecc_log record.
-- =============================================
CREATE PROCEDURE usp_proposal_tecc_log_select
	@proposal_id INT,
	@employee_id INT,
	@transmitted_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_tecc_log].*
	FROM
		[dbo].[proposal_tecc_log] WITH(NOLOCK)
	WHERE
		[proposal_id]=@proposal_id
		AND [employee_id]=@employee_id
		AND [transmitted_date]=@transmitted_date
END

