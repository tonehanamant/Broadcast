

CREATE PROCEDURE [dbo].[usp_BRS_GetAdvertiserItems]
@agencyId int

AS
BEGIN

	SET NOCOUNT ON;

	SELECT id,
			[name]
	FROM
		cmw_traffic_company_companies (nolock)
	JOIN
		cmw_traffic_companies (nolock) on cmw_traffic_companies.id = cmw_traffic_company_companies.child_cmw_traffic_company_id
	WHERE
		cmw_traffic_company_companies.parent_cmw_traffic_company_id = @agencyId
	ORDER BY [name] ASC
END

