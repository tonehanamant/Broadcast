
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCompanyItemsForCompany]
	@parent_company_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @is_agency AS INT
	DECLARE @is_advertiser AS INT

	SET @is_agency = (
		SELECT COUNT(*) FROM company_company_types (NOLOCK) WHERE company_id=@parent_company_id AND company_type_id=1
	)
	SET @is_advertiser = (
		SELECT COUNT(*) FROM company_company_types (NOLOCK) WHERE company_id=@parent_company_id AND company_type_id=2
	)

    SELECT 
		c.id,
		c.name 
	FROM 
		companies c (NOLOCK)
	WHERE 
		c.id IN (
			SELECT company_id FROM company_companies (NOLOCK) WHERE parent_company_id=@parent_company_id
		)
		OR
		(
			@is_agency=1 AND @is_advertiser=1 AND c.id=@parent_company_id
		)
	ORDER BY 
		c.name
END

