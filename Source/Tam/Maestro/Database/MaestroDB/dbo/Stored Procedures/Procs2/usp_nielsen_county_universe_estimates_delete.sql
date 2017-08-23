CREATE PROCEDURE [dbo].[usp_nielsen_county_universe_estimates_delete]
(
	@document_id		Int,
	@line_number		Int)
AS
DELETE FROM
	nielsen_county_universe_estimates
WHERE
	document_id = @document_id
 AND
	line_number = @line_number
