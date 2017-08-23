CREATE PROCEDURE usp_reel_advertisers_delete
(
	@reel_id		Int,
	@line_number		Int)
AS
DELETE FROM
	reel_advertisers
WHERE
	reel_id = @reel_id
 AND
	line_number = @line_number
