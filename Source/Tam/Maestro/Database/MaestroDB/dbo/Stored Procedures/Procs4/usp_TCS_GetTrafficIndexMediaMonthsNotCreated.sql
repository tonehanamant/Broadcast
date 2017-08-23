

-- =============================================
-- Author:        <Author,,Name>
-- Create date: <Create Date,,>
-- Description:   <Description,,>
-- =============================================

CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficIndexMediaMonthsNotCreated]

AS

select id, year, month, media_month, start_date, end_date from 
	media_months (NOLOCK) where id not in
(select media_month_id from traffic_index_index)
order by id


