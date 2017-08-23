
CREATE PROCEDURE [dbo].[usp_ARSLoader_GetPreCrunchProcedureForRatingCategory]
	@ratingCategory VARCHAR(15)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT arc.precrunch_stored_procedure
	FROM dbo.autoforecasted_rating_categories arc (NOLOCK)
	JOIN dbo.rating_categories rc (NOLOCK)
		ON arc.rating_category_id = rc.id
	WHERE rc.code = @ratingCategory
END

