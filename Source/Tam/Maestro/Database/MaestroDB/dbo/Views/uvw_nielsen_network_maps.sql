CREATE VIEW [dbo].[uvw_nielsen_network_maps]
AS
	SELECT
		nnm.id 'nielsen_network_map_id',
		nnm.effective_date 'start_date',
		nnm.nielsen_network_id, 
		nnm.map_set, 
		nnm.map_value, 
		nnm.active, 
		NULL 'end_date'
	FROM
		dbo.nielsen_network_maps nnm (NOLOCK)
		
	UNION ALL
	
	SELECT
		nnmh.nielsen_network_map_id, 
		nnmh.start_date, 
		nnmh.nielsen_network_id, 
		nnmh.map_set, 
		nnmh.map_value, 
		nnmh.active, 
		nnmh.end_date
	FROM
		dbo.nielsen_network_map_histories nnmh (NOLOCK)
