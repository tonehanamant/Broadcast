
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayCompany]
	@company_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		companies.id,
		companies.name,
		(SELECT COUNT(*) FROM company_company_types WHERE company_id=companies.id AND company_type_id=2),
		(SELECT COUNT(*) FROM company_company_types WHERE company_id=companies.id AND company_type_id=1),
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
		companies.id=@company_id
	ORDER BY
		companies.name
END