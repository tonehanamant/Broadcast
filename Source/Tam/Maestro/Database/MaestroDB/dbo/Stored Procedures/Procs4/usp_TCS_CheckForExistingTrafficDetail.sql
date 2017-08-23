CREATE PROCEDURE usp_TCS_CheckForExistingTrafficDetail
(
                @source_traffic_detail_id int,
                @daypart_id int
)
AS

declare @traffic_id int;
declare @network_id int;

SELECT @network_id = network_id, @traffic_id = traffic_id from
traffic_details td with (NOLOCK)
where td.id = @source_traffic_detail_id;

SELECT
                td.id 
FROM
                traffic_details td with (nolock) 
where 
                td.network_id = @network_id and 
                td.daypart_id = @daypart_id and
                td.traffic_id = @traffic_id;
