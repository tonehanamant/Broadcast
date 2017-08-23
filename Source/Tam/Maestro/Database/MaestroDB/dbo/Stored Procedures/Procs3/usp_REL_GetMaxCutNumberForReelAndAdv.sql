CREATE PROCEDURE [dbo].[usp_REL_GetMaxCutNumberForReelAndAdv]
      @reel_id INT,
      @line_number int
AS
BEGIN
    SELECT
            isnull(max(cut), 0)
      FROM
            reel_materials rm WITH (NOLOCK) 
      WHERE
            rm.reel_id=@reel_id
            and rm.line_number = @line_number
END
