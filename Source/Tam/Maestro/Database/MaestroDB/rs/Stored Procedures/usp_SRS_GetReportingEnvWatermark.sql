
CREATE PROCEDURE [rs].[usp_SRS_GetReportingEnvWatermark]

/* -----------------------------------------------------------------------------------------------

 Modifications Log:
	- object: usp_SRS_GetReportingEnvWatermark
	- Coder : Rich Sara
	- Date  : 3/18/2011
	- ModID : n\a 
	- Narrative for changes made: 
	-  1: Initial Creation 

 To execute:  
   Exec rs.usp_SRS_GetReportingEnvWatermark  
   
------------------------------------------------------------------------------------------------ */
 
AS

BEGIN

Set NoCount ON

SELECT TOP 1 
				CASE SPECIFIC_CATALOG
					WHEN 'cmw_reporting_dev' THEN 'NOTE: Report was run against DEVELOPMENT database ... '
					WHEN 'cmw_reporting_qa'  THEN 'NOTE: Report was run against QUALITY ASSURANCE database ... '
					ELSE ''
				END  'env_name'
FROM INFORMATION_SCHEMA.ROUTINES
 
--------------------------------------------------------------------------------------------------------------------- 

END