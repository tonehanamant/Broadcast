CREATE FUNCTION [dbo].[GetProductsForMarriedProposal]
(
	@proposal_id INT
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @return AS VARCHAR(MAX)
	DECLARE @is_married AS INT

	SET @is_married = (
		SELECT COUNT(*) FROM proposal_proposals WITH (NOLOCK) WHERE parent_proposal_id = @proposal_id
	);

	IF @is_married = 0
		BEGIN
			SET @return = (
				SELECT 
					p.name 
				FROM 
					proposals m (NOLOCK) 
					LEFT JOIN products p (NOLOCK) ON p.id=m.product_id 
				WHERE
					m.id=@proposal_id
			)
		END
	ELSE
		BEGIN
			DECLARE @product AS VARCHAR(MAX)
			DECLARE ProductCursor CURSOR FAST_FORWARD FOR
			select 
				pr.name
			from 
				proposal_proposals pp WITH (NOLOCK) join
				proposals p WITH(NOLOCK) on p.id = pp.child_proposal_id	LEFT join
				products pr WITH (NOLOCK) on pr.id = p.product_id
			WHERE
				pp.parent_proposal_id = @proposal_id

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
