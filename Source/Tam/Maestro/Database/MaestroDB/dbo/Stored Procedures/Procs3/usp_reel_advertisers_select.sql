CREATE PROCEDURE usp_reel_advertisers_select
(
	@reel_id		Int,
	@line_number		Int
)
AS
SELECT
	*
FROM
	reel_advertisers WITH(NOLOCK)
WHERE
	reel_id=@reel_id
	AND
	line_number=@line_number

