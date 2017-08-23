-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/25/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayProposals_ForEmployee]
	@employee_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		dp.*
	FROM 
		uvw_display_proposals dp
		JOIN proposals p (NOLOCK) ON p.id=dp.id
	WHERE
		p.salesperson_employee_id=@employee_id
		AND p.proposal_status_id<>7
	ORDER BY 
		dp.id DESC
END
