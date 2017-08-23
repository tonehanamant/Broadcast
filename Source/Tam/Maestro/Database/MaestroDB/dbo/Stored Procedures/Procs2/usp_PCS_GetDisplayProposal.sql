-- usp_PCS_GetDisplayProposal 28023
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayProposal]
	@proposal_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		dp.*
	FROM 
		uvw_display_proposals dp
	WHERE 
		dp.id=@proposal_id
END
