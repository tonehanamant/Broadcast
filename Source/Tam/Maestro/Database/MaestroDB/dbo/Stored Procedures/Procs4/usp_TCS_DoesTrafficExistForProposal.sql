-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_DoesTrafficExistForProposal
(
	@proposal_id int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		proposal_id 
	FROM 
		traffic_proposals (NOLOCK) 
	WHERE 
		proposal_id = @proposal_id
END
