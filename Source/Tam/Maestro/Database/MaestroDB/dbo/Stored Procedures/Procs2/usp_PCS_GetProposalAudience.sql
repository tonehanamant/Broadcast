-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/10/2012
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetProposalAudience 39350,1
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalAudience]
	@proposal_id INT,
	@ordinal INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		pa.*
	FROM 
		proposal_audiences pa (NOLOCK) 
	WHERE 
		pa.proposal_id=@proposal_id
		AND pa.ordinal=@ordinal
END
