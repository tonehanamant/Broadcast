-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetContactsByCompany]
	@company_id INT
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
		c.company_id=@company_id 
	ORDER BY 
		c.last_name

END
