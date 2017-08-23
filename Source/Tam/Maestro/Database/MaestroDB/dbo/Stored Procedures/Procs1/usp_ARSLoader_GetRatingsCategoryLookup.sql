-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/17/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ARSLoader_GetRatingsCategoryLookup
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		rc.id,
		rc.code
	FROM 
		rating_categories rc (NOLOCK)
END
