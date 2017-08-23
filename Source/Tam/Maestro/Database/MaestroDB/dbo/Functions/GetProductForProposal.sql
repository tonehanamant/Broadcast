-- =============================================
-- Author:		Stephen DeFusco
-- ALTER date:   8/23/2010
-- Description:	Handles married and non-married proposals.
-- =============================================
CREATE FUNCTION [dbo].[GetProductForProposal]
(
	@proposal_id INT
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @return AS VARCHAR(MAX)
	DECLARE @is_married AS BIT

	SET @is_married = (
		SELECT CASE WHEN COUNT(1)>0 THEN 1 ELSE 0 END FROM proposal_proposals pp (NOLOCK) WHERE pp.parent_proposal_id=@proposal_id
	);

	IF @is_married = 0
		BEGIN
			SET @return = (
				SELECT 
					pr.name 
				FROM 
					proposals p (NOLOCK)
					JOIN products pr (NOLOCK) ON pr.id=p.product_id 
				WHERE
					p.id=@proposal_id
			)
		END
	ELSE
		BEGIN
			DECLARE @product AS VARCHAR(MAX)
			DECLARE ProductCursor CURSOR FAST_FORWARD FOR
				SELECT 
					pr.name
				FROM 
					proposal_proposals pp (NOLOCK)
					JOIN proposals p (NOLOCK) ON p.id=pp.child_proposal_id
					JOIN products pr (NOLOCK) ON pr.id=p.product_id
				WHERE
					pp.parent_proposal_id=@proposal_id
				ORDER BY
					pp.ordinal

			SET @return = ''

			OPEN ProductCursor

			FETCH NEXT FROM ProductCursor INTO @product
			WHILE @@FETCH_STATUS = 0
				BEGIN
					IF CHARINDEX(@product,@return) = 0
					BEGIN
						SET @return = @return + @product
						SET @return = @return + ' / '
					END
					FETCH NEXT FROM ProductCursor INTO @product
				END

			-- strip last character
			IF LEN(@return) > 2
				SET @return = LEFT(@return, LEN(@return) - 2)

			CLOSE ProductCursor
			DEALLOCATE ProductCursor
		END
		
	RETURN @return;
END

