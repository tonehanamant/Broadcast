-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetCompanyDetailItems]
	@company_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		companies.name,
		companies.url,
		company_statuses.name,
		billing_terms.name,
		companies.additional_information,
		companies.enabled,
		companies.date_created,
		companies.date_last_modified 
	FROM 
		companies (NOLOCK)
		LEFT JOIN company_statuses (NOLOCK) ON company_statuses.id=companies.company_status_id 
		LEFT JOIN billing_terms (NOLOCK) ON billing_terms.id=companies.default_billing_terms_id 
	WHERE 
		companies.id=@company_id

	SELECT 
		company_types.name 
	FROM 
		company_company_types (NOLOCK) 
		LEFT JOIN company_types (NOLOCK) ON company_types.id=company_company_types.company_type_id 
	WHERE 
		company_id=@company_id

	SELECT 
		address_types.name,
		addresses.address_line_1,
		addresses.city,
		states.code,
		addresses.zip 
	FROM 
		addresses (NOLOCK) 
		LEFT JOIN address_types (NOLOCK) ON address_types.id=addresses.address_type_id 
		LEFT JOIN states (NOLOCK) ON states.id=addresses.state_id 
	WHERE 
		addresses.id IN (
			SELECT address_id FROM company_addresses (NOLOCK) WHERE company_id=@company_id
		)
END
