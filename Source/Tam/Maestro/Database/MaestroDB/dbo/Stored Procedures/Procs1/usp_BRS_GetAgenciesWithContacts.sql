


CREATE PROCEDURE [dbo].[usp_BRS_GetAgenciesWithContacts]

AS
BEGIN

	SET NOCOUNT ON;

	SELECT DISTINCT
		cmw_traffic_companies.id,
		cmw_traffic_companies.[name]
	FROM
		cmw_traffic (nolock)
	JOIN
		cmw_traffic_companies (nolock) on cmw_traffic_companies.id = cmw_traffic.agency_cmw_traffic_company_id
	JOIN
		cmw_traffic_agencies (nolock) on cmw_traffic_companies.id = cmw_traffic_agencies.cmw_traffic_company_id
	JOIN
		cmw_contacts (nolock) on cmw_traffic_companies.id = cmw_contacts.cmw_traffic_company_id
	ORDER BY 
		cmw_traffic_companies.[name] ASC
END


