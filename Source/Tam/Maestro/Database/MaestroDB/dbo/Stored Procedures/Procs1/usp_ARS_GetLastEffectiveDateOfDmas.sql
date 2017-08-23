-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/20/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ARS_GetLastEffectiveDateOfDmas
AS
BEGIN
	SET NOCOUNT ON;

	SELECT MAX(d.effective_date) FROM dbo.dmas d (NOLOCK)
END
