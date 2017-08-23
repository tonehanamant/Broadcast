-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetContactsForContacts]
	@ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		c.*
	FROM 
		contacts c (NOLOCK) 
	WHERE 
		c.id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
