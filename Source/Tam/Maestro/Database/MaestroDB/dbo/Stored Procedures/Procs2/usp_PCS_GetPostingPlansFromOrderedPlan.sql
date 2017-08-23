-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/9/2010
-- Description:	Returns all posting plans associated with the specified ordered plan.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetPostingPlansFromOrderedPlan]
	@ordered_proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		p.*
	FROM
		proposals p WITH(NOLOCK)
	WHERE
		p.original_proposal_id=@ordered_proposal_id
END
