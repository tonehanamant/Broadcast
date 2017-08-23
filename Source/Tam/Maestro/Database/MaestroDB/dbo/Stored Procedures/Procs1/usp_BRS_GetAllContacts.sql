



CREATE PROCEDURE [dbo].[usp_BRS_GetAllContacts]

AS

BEGIN

	SET NOCOUNT ON;

SELECT
	c.id,
	c.first_name,
	c.last_name,
	cmw_traffic_companies.[name] 'Agency',
	salutations.[name] 'Salutation',
	c.title,
	c.department,
	c.assistant,
	c.assistant_title,
	c.address_line_1,
	c.address_line_2,
	c.address_line_3,
	c.city,
	states.[code] 'State',
	c.zip,
	countries.[code] 'Country',
	c.date_created,
	c.date_last_modified
FROM
	cmw_contacts c (NOLOCK)
JOIN
	cmw_traffic_agencies (nolock) on c.cmw_traffic_company_id = cmw_traffic_agencies.cmw_traffic_company_id
LEFT JOIN
	states (nolock) on c.state_id = states.id
JOIN
	countries (nolock) on c.country_id = countries.id
JOIN
	cmw_traffic_companies (nolock) on c.cmw_traffic_company_id = cmw_traffic_companies.id
LEFT JOIN
	salutations (nolock) on c.salutation_id = salutations.id
	ORDER BY
		c.last_name ASC
END



