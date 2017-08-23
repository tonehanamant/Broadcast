
CREATE FUNCTION [dbo].[GetProposalDetailRating]
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
			rating 
		FROM 
			proposal_detail_audiences (NOLOCK)
		WHERE 
			proposal_detail_id=@proposal_detail_id
			AND audience_id=@audience_id
	)

	RETURN @return;
END
