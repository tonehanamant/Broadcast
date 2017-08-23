
CREATE VIEW [dbo].[uvw_nielsen_network_rating_dayparts]
AS
	SELECT 
		nnrd.nielsen_network_id, 
		nnrd.daypart_id, 
		nnrd.effective_date 'start_date', 
		NULL 'end_date' 
	FROM 
		dbo.nielsen_network_rating_dayparts nnrd (NOLOCK)
		
	UNION
	
	SELECT 
		nnrdh.nielsen_network_id, 
		nnrdh.daypart_id, 
		nnrdh.[start_date], 
		nnrdh.end_date 
	FROM 
		dbo.nielsen_network_rating_daypart_histories nnrdh (NOLOCK)

