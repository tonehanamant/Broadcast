	CREATE PROCEDURE [dbo].[usp_REL_GetUnmarriedMaterials]    
	AS  
	BEGIN  
	 SELECT m.*  
	 FROM materials m (NOLOCK)  
	 WHERE m.[type] <> 'Married'
	 ORDER BY m.code  
	END
