-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_BRS_GetReportFileNameByOrderID]
@OrderID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @originalCMWTrafficID int
	set @originalCMWTrafficID = (select original_cmw_traffic_id from cmw_traffic where id = @OrderID)
	IF @originalCMWTrafficID IS NULL 
		BEGIN
			set @originalCMWTrafficID = @OrderID
		END

	SELECT
		cmw_traffic.id,
		CAST(@originalCMWTrafficID AS varchar) 
		+ '_' +  systems.code + '_' + advertisers.[name]  
		+ '_' + convert(varchar,cmw_traffic.start_date,112)
		+ '_Rev' + CAST(cmw_traffic.version_number AS varchar)
		AS 'Report_Name'

	FROM
		cmw_traffic (nolock)
	JOIN
		systems (nolock) on systems.id = cmw_traffic.system_id
	JOIN 
		cmw_traffic_companies advertisers (NOLOCK) ON advertisers.id=cmw_traffic.advertiser_cmw_traffic_company_id 
	WHERE
		cmw_traffic.id = @OrderID
END
