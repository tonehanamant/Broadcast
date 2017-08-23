CREATE PROCEDURE usp_reel_advertisers_update
(
	@reel_id		Int,
	@line_number		Int,
	@display_name		VarChar(255)
)
AS
UPDATE reel_advertisers SET
	display_name = @display_name
WHERE
	reel_id = @reel_id AND
	line_number = @line_number
