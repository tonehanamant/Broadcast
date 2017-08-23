
CREATE PROCEDURE [dbo].[usp_REL_GetMaxLineNumberForReel]
	@reel_id INT
AS
BEGIN
    SELECT
		max(line_number)
	FROM
		reel_advertisers ra WITH (NOLOCK) 
	WHERE
		ra.reel_id=@reel_id
END
