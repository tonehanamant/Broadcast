

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetRatingsBaseMediaMonthsForTraffic]
AS
BEGIN
	select 
		media_months.id 
	from properties 
		join media_months on media_months.media_month = properties.value 
	where 
		name = 'latest_base_month'
END
