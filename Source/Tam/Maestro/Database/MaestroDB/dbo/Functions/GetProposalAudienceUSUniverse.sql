-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalAudienceUSUniverse]
(
	@proposal_id INT,
	@audience_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT
	
	SET @return = (
		SELECT 
			universe 
		FROM 
			proposal_audiences pa (NOLOCK) 
		WHERE 
			proposal_id=@proposal_id
			AND audience_id=@audience_id
	)
	
	RETURN @return
END
