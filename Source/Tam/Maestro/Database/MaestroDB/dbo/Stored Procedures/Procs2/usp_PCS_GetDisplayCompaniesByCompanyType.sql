
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayCompaniesByCompanyType]
	@company_type_id INT
AS
BEGIN
    SELECT 
		companies.id,
		companies.name,
		(SELECT COUNT(*) FROM company_company_types (NOLOCK) WHERE company_id=companies.id AND company_type_id=2),
		(SELECT COUNT(*) FROM company_company_types (NOLOCK) WHERE company_id=companies.id AND company_type_id=1),
		company_statuses.name 'company_status',
		companies.default_rate_card_type_id,
		account_statuses.name,
		companies.enabled,
		date_created,
		date_last_modified
	FROM 
		companies (NOLOCK) 
		LEFT JOIN account_statuses (NOLOCK) ON account_statuses.id=companies.account_status_id 
		LEFT JOIN company_statuses (NOLOCK) ON company_statuses.id=companies.company_status_id
	WHERE
		companies.id IN (
			SELECT company_id FROM company_company_types (NOLOCK) WHERE company_type_id=@company_type_id
		)
END
