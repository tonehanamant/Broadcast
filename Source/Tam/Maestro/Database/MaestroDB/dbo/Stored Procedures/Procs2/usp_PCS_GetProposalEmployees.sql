-- =============================================
-- Author:		Stephen DeFusco	
-- Create date: 12/16/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetProposalEmployees 26591
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalEmployees]
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		pe.*
	FROM 
		proposal_employees pe (NOLOCK) 
	WHERE 
		pe.proposal_id=@proposal_id
	ORDER BY
		pe.effective_date ASC
END
