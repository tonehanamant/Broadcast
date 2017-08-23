CREATE PROCEDURE [dbo].[usp_nielsen_ndas_select]
(
	@document_id		Int,
	@line_number		Int
)
AS
SELECT
	*
FROM
	nielsen_ndas WITH(NOLOCK)
WHERE
	document_id=@document_id
	AND
	line_number=@line_number
