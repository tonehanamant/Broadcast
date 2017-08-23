CREATE PROCEDURE [dbo].[usp_nielsen_ndas_delete]
(
	@document_id		Int,
	@line_number		Int)
AS
DELETE FROM
	nielsen_ndas
WHERE
	document_id = @document_id
 AND
	line_number = @line_number
