

-- =============================================
-- Author:        <Author,,Name>
-- Create date: <Create Date,,>
-- Description:   <Description,,>
-- =============================================

CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficIndexIndexByMediaMonth]
( 
	@media_month_id as int 
)
AS

select id, media_month_id from traffic_index_index (NOLOCK) where media_month_id = @media_month_id


