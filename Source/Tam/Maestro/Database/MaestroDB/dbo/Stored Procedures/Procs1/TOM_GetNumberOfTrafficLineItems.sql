CREATE PROCEDURE [dbo].[TOM_GetNumberOfTrafficLineItems]
	@traffic_detail_id as int
AS
BEGIN
	with myview as (
		select distinct 
			network_id, daypart_id, start_date 
		from 
			traffic_details td (NOLOCK)
			JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.traffic_detail_id=td.id
				AND tdw.traffic_detail_id=@traffic_detail_id
		where 
			td.id = @traffic_detail_id
	)
	
	select count(*) from myview
END
