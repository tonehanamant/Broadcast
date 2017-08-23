

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_BRS_GetAllAdvertiserItems]
AS
BEGIN

	SET NOCOUNT ON;
    
	SELECT DISTINCT
		cmw_traffic_companies.id,
		cmw_traffic_companies.[name]
	FROM
		cmw_traffic_advertisers (nolock)
	JOIN
		cmw_traffic_companies (nolock) on cmw_traffic_companies.id = cmw_traffic_advertisers.cmw_traffic_company_id
	ORDER BY
		cmw_traffic_companies.[name] ASC
END


