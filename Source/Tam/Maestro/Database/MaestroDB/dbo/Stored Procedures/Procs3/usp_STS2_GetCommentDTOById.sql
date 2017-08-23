

-- =============================================
-- Author:		John Carsley
-- Create date: 06/191/2012
-- Description:	Gets comments by form name
-- Usage: exec usp_STS2_usp_STS2_GetCommentBOById 1
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetCommentDTOById]
     @id int
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT c.[id]
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
  WHERE c.[id] = @id
END


