
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalItemsForEmployee]
	@employee_id INT
AS
BEGIN
	SELECT 
		id,
		name 
	FROM 
		proposals (NOLOCK) 
	WHERE 
		original_proposal_id IS NULL 
		AND salesperson_employee_id=@employee_id
		AND proposals.id NOT IN (
			SELECT DISTINCT pp.parent_proposal_id FROM proposal_proposals pp (NOLOCK)
		)
	ORDER BY 
		id DESC
END
