CREATE PROCEDURE [dbo].[usp_REL_GetAllMaterialsByIscii]  
AS
BEGIN
	select 
		m.*
	from
		materials m (NOLOCK)
	order by
		m.code
END