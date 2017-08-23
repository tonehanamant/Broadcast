-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 03/22/2016
-- Description:	Returns the last recorded daily IMS health score for a proposal.
-- =============================================
/*
	EXEC usp_PCS_GetLastDailyImsHealthScore 64248
*/
CREATE PROCEDURE [dbo].[usp_PCS_GetLastDailyImsHealthScore]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	SELECT TOP 1
		pic.*
	FROM
		dbo.proposal_inventory_checks pic
	WHERE
		pic.proposal_id=@proposal_id
	ORDER BY
		pic.date_created DESC;
END