-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/27/2012
-- Description:	
-- =============================================
CREATE PROCEDURE usp_ARS_DoesDocumentExist
	@hash VARCHAR(63)
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		COUNT(d.id) 
	FROM 
		dbo.documents d (NOLOCK) 
	WHERE 
		d.[hash]=@hash
END
