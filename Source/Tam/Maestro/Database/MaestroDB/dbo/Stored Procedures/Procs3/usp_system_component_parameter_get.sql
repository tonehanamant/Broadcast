CREATE PROCEDURE usp_system_component_parameter_get        
(        
	@component_id int        
	,@parameter_key varchar(63)      
)        
AS         
	SELECT         
		parameter_value
	FROM        
		system_component_parameters WITH(NOLOCK)
	WHERE component_id = @component_id
		AND parameter_key = @parameter_key
		       
