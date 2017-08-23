	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 3/13/2015
	-- Description:	Given an ordered proposal ID get's the appropriate official post.
	-- =============================================
	-- Example:
	-- SELECT dbo.udf_GetTamPostByOrderedProposalId(60489)
	CREATE FUNCTION [dbo].[udf_GetTamPostByOrderedProposalId]
	(
		@ordered_proposal_id INT
	)
	RETURNS INT
	AS
	BEGIN
		DECLARE @return INT;
	
		WITH ordered (tam_post_id,ordinal) AS (
			SELECT
				tp.id,
				ROW_NUMBER() OVER (ORDER BY tp.date_created) 'ordinal'
			FROM
				tam_posts tp (NOLOCK)
			WHERE
				tp.id IN (
					SELECT
						tpp.tam_post_id
					FROM
						tam_post_proposals tpp (NOLOCK)
						JOIN proposals pp (NOLOCK) ON pp.id=tpp.posting_plan_proposal_id
						JOIN tam_posts tp (NOLOCK) ON tp.id=tpp.tam_post_id
							AND tp.is_deleted=0
							AND tp.post_type_code=1
					WHERE
						pp.original_proposal_id=@ordered_proposal_id
						AND tpp.post_completed IS NOT NULL
						AND tpp.aggregation_completed IS NOT NULL
						AND tpp.aggregation_status_code=1
					GROUP BY
						tpp.tam_post_id
				)
		)
		SELECT 
			@return = o.tam_post_id 
		FROM 
			ordered o 
		WHERE 
			o.ordinal=1;
		
		RETURN @return;
	END
