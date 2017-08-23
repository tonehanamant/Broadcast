CREATE VIEW [dbo].[uvw_network_substitutions]
AS
	SELECT
		network_id, 
		substitution_category_id,
		substitute_network_id,
		weight,
		effective_date 'start_date', 
		NULL 'end_date',
		rating_category_group_id
	FROM
		dbo.network_substitutions (NOLOCK)
	
	UNION ALL
	
	SELECT     
		network_id, 
		substitution_category_id,
		substitute_network_id,
		weight, 
		start_date, 
		end_date,
		rating_category_group_id
	FROM
		dbo.network_substitution_histories (NOLOCK)
