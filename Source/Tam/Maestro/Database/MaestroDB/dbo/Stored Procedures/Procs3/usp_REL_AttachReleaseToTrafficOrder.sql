
CREATE Procedure [dbo].[usp_REL_AttachReleaseToTrafficOrder]
(
	@traffic_id int,
	@release_id int
)
AS

update traffic_orders set release_id = @release_id where traffic_detail_id in (select id from traffic_details where traffic_id = @traffic_id)

