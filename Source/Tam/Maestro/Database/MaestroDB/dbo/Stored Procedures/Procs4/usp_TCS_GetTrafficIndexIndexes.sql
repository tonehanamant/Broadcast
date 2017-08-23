

-- =============================================
-- Author:        <Author,,Name>
-- Create date: <Create Date,,>
-- Description:   <Description,,>
-- =============================================

CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficIndexIndexes]

AS

select id, media_month_id from traffic_index_index (NOLOCK)
order by media_month_id


