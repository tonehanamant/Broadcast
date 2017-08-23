


CREATE Procedure [dbo].[usp_TCS_DoesReleaseOrderExist]
(
	@original_id int
)
AS

select id from traffic (NOLOCK) where original_traffic_id = @original_id and status_id = 24;


