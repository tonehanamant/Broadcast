-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/14/2014
-- Description:	
-- =============================================
CREATE FUNCTION udf_IsTrafficMarried
(
	@traffic_id INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @is_married BIT;
	SELECT 
		@is_married = CASE WHEN COUNT(1)>1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END 
	FROM
		dbo.proposal_proposals pp (NOLOCK) 
		JOIN dbo.traffic_proposals tp (NOLOCK) ON tp.proposal_id=pp.parent_proposal_id
	WHERE
		tp.traffic_id=@traffic_id;
	RETURN @is_married;
END
