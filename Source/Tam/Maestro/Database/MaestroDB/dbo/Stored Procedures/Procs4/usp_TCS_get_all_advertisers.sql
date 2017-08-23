CREATE PROCEDURE [dbo].[usp_TCS_get_all_advertisers]
AS
select *
from companies
join company_company_types on companies.id = company_company_types.company_id
join company_types on company_company_types.company_type_id = company_types.id
where company_types.name = 'Advertiser'
