-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetCompanyItemsForCompanyType
	@company_type_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		name 
	FROM 
		companies (NOLOCK)
	WHERE 
		id IN (
			SELECT company_id FROM company_company_types (NOLOCK) WHERE company_type_id=@company_type_id
		) 
	ORDER BY 
		name
END
