CREATE PROCEDURE usp_reel_advertisers_insert
(
	@reel_id		Int,
	@line_number		Int,
	@display_name		VarChar(255)
)
AS
INSERT INTO reel_advertisers
(
	reel_id,
	line_number,
	display_name
)
VALUES
(
	@reel_id,
	@line_number,
	@display_name
)

