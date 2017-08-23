
CREATE VIEW [dbo].[uvw_spot_length_maps]
AS
	SELECT
		id 'spot_length_map_id', 
		spot_length_id, 
		map_set, 
		map_value, 
		active, 
		effective_date 'start_date',
		NULL 'end_date'
	FROM         
		dbo.spot_length_maps (NOLOCK)
		
	UNION ALL
	
	SELECT     
		spot_length_map_id, 
		spot_length_id, 
		map_set, 
		map_value, 
		active, 
		start_date, 
		end_date
	FROM         
		dbo.spot_length_map_histories (NOLOCK)
