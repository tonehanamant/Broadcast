
CREATE PROC [dbo].[usp_REL2_GetReleaseMsos]
AS
BEGIN
	SELECT [id]
      ,[code]
      ,[name]
      ,[type]
      ,[active]
      ,[effective_date]
  FROM [dbo].[businesses]
  WHERE [type] = 'Release Mso'
END
