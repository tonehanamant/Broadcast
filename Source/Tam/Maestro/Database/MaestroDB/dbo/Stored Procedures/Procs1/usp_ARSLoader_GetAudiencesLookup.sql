-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/17/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ARSLoader_GetAudiencesLookup
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		a.id,
		a.code 
	FROM 
		audiences a (NOLOCK)
	WHERE 
		a.custom=0
END
