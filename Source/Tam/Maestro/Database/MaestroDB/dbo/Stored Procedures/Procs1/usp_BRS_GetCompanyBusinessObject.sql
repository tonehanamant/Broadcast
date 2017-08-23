




CREATE PROCEDURE [dbo].[usp_BRS_GetCompanyBusinessObject]
@companyID int

AS
BEGIN

	SET NOCOUNT ON;

	exec dbo.usp_cmw_traffic_companies_select @companyID
	exec dbo.usp_cmw_traffic_advertisers_select @companyID
	exec dbo.usp_cmw_traffic_agencies_select @companyID
	SELECT
			parent_cmw_traffic_company_id,
			[name]
	FROM
		cmw_traffic_company_companies (nolock)
	JOIN
		cmw_traffic_companies (nolock) on cmw_traffic_companies.id = cmw_traffic_company_companies.parent_cmw_traffic_company_id
	WHERE
		child_cmw_traffic_company_id = @companyID
	
	SELECT
			id,
			cmw_traffic_advertisers_id,
			[name]
	FROM
		cmw_traffic_products (NOLOCK)
	WHERE
		cmw_traffic_advertisers_id = @companyID

	SELECT
			child_cmw_traffic_company_id,
			[name]
	FROM
		cmw_traffic_company_companies (nolock)
	JOIN
		cmw_traffic_companies (nolock) on cmw_traffic_companies.id = cmw_traffic_company_companies.child_cmw_traffic_company_id
	WHERE
		parent_cmw_traffic_company_id = @companyID
			
END




