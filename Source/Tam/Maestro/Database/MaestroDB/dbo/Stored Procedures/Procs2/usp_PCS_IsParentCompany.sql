-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_IsParentCompany]
	@company_id INT,
	@parent_company_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		COUNT(*) 
	FROM 
		company_companies (NOLOCK) 
	WHERE 
		company_id=@company_id 
		AND parent_company_id=@parent_company_id
END
