
create PROCEDURE [rs].[usp_SRS_GetSalesModels]

AS

/* -----------------------------------------------------------------------------------------------

 Modifications Log:
	- object: usp_SRS_GetSalesModels
	- Coder : Rich Sara
	- Date  : 2/16/2011 
	- ModID : n\a 
	- Narrative for changes made: 
	-  1: Initial Creation ; used for Traffic Reporting 

 To execute:  
   Exec rs.usp_SRS_GetSalesModels  

------------------------------------------------------------------------------------------------ */

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

BEGIN 
	
	SELECT 
		id, name  
	FROM        
		sales_models 
	ORDER BY
		name 
		
END



