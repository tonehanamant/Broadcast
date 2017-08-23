-- usp_REL_GetRollupTrafficDetails 207
CREATE PROCEDURE [dbo].[usp_REL_AttachReleaseToTrafficOrdersTable]
	@traffic_id int,
	@release_id int
AS
BEGIN
	update 
		traffic_orders 
	set 
		release_id = @release_id 
	where 
		traffic_orders.traffic_detail_id in (
			select id from traffic_details where traffic_id = @traffic_id
		)
	
	-- added to clear all traffic_dollars_allocation_lookup records which have traffic orders that were removed from releases
	IF @release_id IS NULL
	BEGIN
		DELETE
			tdal
		FROM 
			traffic_dollars_allocation_lookup tdal
			JOIN traffic t (NOLOCK) ON t.id=tdal.traffic_id 
				AND t.release_id IS NULL 
		WHERE 
			tdal.release_dollars_allocated>0
	END
END