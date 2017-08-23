CREATE PROCEDURE [dbo].[usp_nielsen_county_universe_estimates_select]
(
	@document_id		Int,
	@line_number		Int
)
AS
SELECT
	*
FROM
	nielsen_county_universe_estimates WITH(NOLOCK)
WHERE
	document_id=@document_id
	AND
	line_number=@line_number
