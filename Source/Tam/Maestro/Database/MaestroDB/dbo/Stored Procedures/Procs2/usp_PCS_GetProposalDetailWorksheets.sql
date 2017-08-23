-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalDetailWorksheets]
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		pda.*
	FROM
		proposal_detail_worksheets pda (NOLOCK) 
	WHERE
		pda.proposal_detail_id IN (
			SELECT id FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id
		)
END
