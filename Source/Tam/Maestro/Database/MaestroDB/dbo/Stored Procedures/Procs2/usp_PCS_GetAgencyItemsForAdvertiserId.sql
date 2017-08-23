-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetAgencyItemsForAdvertiserId 40194
CREATE PROCEDURE [dbo].[usp_PCS_GetAgencyItemsForAdvertiserId]
	@company_id INT
AS
BEGIN
	SELECT 
		parent_company_id,
		companies.name
	FROM 
		company_companies	(NOLOCK)
		JOIN companies		(NOLOCK) ON companies.id=company_companies.parent_company_id
	WHERE 
		company_id=@company_id
END
