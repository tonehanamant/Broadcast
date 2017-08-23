



CREATE PROCEDURE [dbo].[usp_BRS_GetAgencyByContactID]
@contactID int
AS
BEGIN

	SET NOCOUNT ON;

	SELECT
		cmw_traffic_companies.id,
		cmw_traffic_companies.[name]
	FROM
		cmw_contacts (nolock)
	JOIN
		cmw_traffic_companies (nolock) on cmw_traffic_companies.id = cmw_contacts.cmw_traffic_company_id
	WHERE
		cmw_contacts.id = @contactID
END
