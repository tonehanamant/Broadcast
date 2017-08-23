
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalsByIds]
	@ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		*
	FROM 
		proposals (NOLOCK)
	WHERE 
		id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		) 
	ORDER BY 
		id DESC
END

