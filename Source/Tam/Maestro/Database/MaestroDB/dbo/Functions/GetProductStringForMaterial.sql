-- =============================================
-- Author:		Stephen DeFusco
-- ALTER date:   8/23/2010
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProductStringForMaterial]
(
	@material_id INT
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @return AS VARCHAR(MAX)
	DECLARE @is_married AS BIT

	SET @is_married = (
		SELECT CASE m.type WHEN 'MARRIED' THEN 1 ELSE 0 END FROM materials m (NOLOCK) WHERE m.id=@material_id
	);

	IF @is_married = 0
		BEGIN
			SET @return = (
				SELECT 
					p.name 
				FROM 
					materials m (NOLOCK) 
					JOIN products p (NOLOCK) ON p.id=m.product_id 
				WHERE
					m.id=@material_id
			)
		END
	ELSE
		BEGIN
			DECLARE @product AS VARCHAR(MAX)
			DECLARE ProductCursor CURSOR FAST_FORWARD FOR
				SELECT 
					p.name
				FROM 
					material_revisions mr	(NOLOCK)
					JOIN materials m		(NOLOCK) ON m.id=mr.revised_material_id
					LEFT JOIN products p	(NOLOCK) ON p.id=m.product_id 
				WHERE
					mr.original_material_id = @material_id
				ORDER BY
					mr.ordinal

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
