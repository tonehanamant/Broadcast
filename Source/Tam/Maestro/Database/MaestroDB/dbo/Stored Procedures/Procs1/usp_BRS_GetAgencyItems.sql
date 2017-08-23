

CREATE PROCEDURE [dbo].[usp_BRS_GetAgencyItems]

AS
BEGIN

	SET NOCOUNT ON;

	SELECT id,
			[name]
	FROM
		cmw_traffic_agencies (nolock)
	JOIN
		cmw_traffic_companies (nolock) on cmw_traffic_companies.id = cmw_traffic_agencies.cmw_traffic_company_id
	ORDER BY [name] ASC
END

