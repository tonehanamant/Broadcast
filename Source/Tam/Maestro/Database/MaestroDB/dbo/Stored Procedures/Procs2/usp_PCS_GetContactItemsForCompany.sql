-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetContactItemsForCompany
	@company_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		first_name,
		last_name 
	FROM 
		contacts (NOLOCK)
	WHERE 
		company_id=@company_id 
	ORDER BY 
		last_name
END
