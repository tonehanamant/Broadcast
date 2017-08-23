CREATE PROCEDURE [dbo].[usp_MCS_GetReelAdvertisers_ForReel]
	@reel_id INT
AS
BEGIN
    SELECT
		ra.*
	FROM
		reel_advertisers ra WITH (NOLOCK) 
	WHERE
		ra.reel_id=@reel_id
	ORDER BY
		ra.line_number
END
