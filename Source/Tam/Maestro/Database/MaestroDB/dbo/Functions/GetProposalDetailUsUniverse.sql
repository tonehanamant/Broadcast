
CREATE FUNCTION [dbo].[GetProposalDetailUsUniverse]
(
	@proposal_detail_id INT,
	@audience_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return FLOAT;

	SET @return = (
		SELECT 
			us_universe 
		FROM 
			proposal_detail_audiences (NOLOCK) 
		WHERE 
			proposal_detail_id=@proposal_detail_id
			AND audience_id=@audience_id
	)

	RETURN @return;
END