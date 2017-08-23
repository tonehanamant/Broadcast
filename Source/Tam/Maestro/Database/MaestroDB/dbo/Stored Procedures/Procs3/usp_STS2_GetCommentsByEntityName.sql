


-- =============================================
-- Author:		John Carsley
-- Create date: 06/191/2012
-- Description:	Gets comments by form name
-- Usage: exec usp_STS2_GetCommentsByEntityName 'SystemsComposer', 'TestForm', null
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetCommentsByEntityName]
     @app_name varchar(100)
	,@form_name varchar(100)
	,@entity_name varchar(100) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
	   c.[id]
      ,[comment_type_id]
      ,[rtf_text]
      ,[plain_text]
      ,[app_name]
      ,[form_name]
      ,[entity_name]
      ,[reference_id]
      ,[modified_date]
      ,[employee_id]
      ,[first_name] = [firstname]
      ,[last_name] = [lastname] 
  FROM [dbo].[comments] c with (nolock)
	inner join [dbo].[employees] e with (nolock) on c.employee_id = e.id
  WHERE [app_name] = @app_name
	and [form_name] = @form_name
	and (([entity_name] = @entity_name) OR (@entity_name IS NULL and entity_name IS NULL)) --handle the case where @entity_name = null
  ORDER BY [modified_date] DESC;
END



