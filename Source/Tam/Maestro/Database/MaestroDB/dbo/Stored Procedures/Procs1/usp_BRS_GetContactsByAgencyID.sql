




CREATE PROCEDURE [dbo].[usp_BRS_GetContactsByAgencyID]
@agencyID int
AS

BEGIN

	SET NOCOUNT ON;

SELECT
	id,
	first_name,
	last_name
FROM
	cmw_contacts (NOLOCK)
JOIN
	cmw_traffic_agencies (nolock) on cmw_contacts.cmw_traffic_company_id = cmw_traffic_agencies.cmw_traffic_company_id
WHERE
	cmw_traffic_agencies.cmw_traffic_company_id = @agencyID
ORDER BY
	cmw_contacts.last_name ASC
END




