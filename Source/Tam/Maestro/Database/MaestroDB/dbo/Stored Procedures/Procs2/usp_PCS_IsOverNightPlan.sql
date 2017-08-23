-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/13/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_IsOverNightPlan
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
	DECLARE @num_overnight_plans INT
	SELECT 
		@num_overnight_plans = COUNT(*) 
	FROM 
		proposals p (NOLOCK) 
	WHERE 
		p.id IN (
			SELECT id FROM dbo.SplitIntegers(@proposal_ids)
		)
		AND dbo.IsOvernightPlan(p.id)=1

	SELECT @num_overnight_plans
END
