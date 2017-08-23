
CREATE PROCEDURE [dbo].[usp_MCS_GetReelAdvertisers_ForReelAndName]
	@reel_id INT,
	@adv_name varchar(255)
AS
BEGIN
    SELECT
		ra.*
	FROM
		reel_advertisers ra WITH (NOLOCK) 
	WHERE
		ra.reel_id=@reel_id
		and ra.display_name = @adv_name
	ORDER BY
		ra.line_number
END
