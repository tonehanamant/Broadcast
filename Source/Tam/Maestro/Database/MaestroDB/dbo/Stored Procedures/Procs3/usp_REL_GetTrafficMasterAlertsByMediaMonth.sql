

CREATE PROCEDURE [dbo].[usp_REL_GetTrafficMasterAlertsByMediaMonth]
(
      @media_month_id int
)
AS

select 
      traffic_master_alerts.*
from 
      media_months (NOLOCK)
      join traffic_master_alerts (NOLOCK) on traffic_master_alerts.date_created >= media_months.start_date
      and
      traffic_master_alerts.date_created <= media_months.end_date
WHERE 
      media_months.id = @media_month_id
ORDER BY
      traffic_master_alerts.name;