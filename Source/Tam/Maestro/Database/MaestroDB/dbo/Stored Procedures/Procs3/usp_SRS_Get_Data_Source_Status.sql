
--IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND 
--       name = 'usp_SRS_Get_Data_Source_Status')
--   DROP  Procedure  usp_SRS_Get_Data_Source_Status
--GO

CREATE PROCEDURE [dbo].[usp_SRS_Get_Data_Source_Status]

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;

Declare @Cnt Int
Set @Cnt = (Select Count(*) FROM v_data_source)

-------------------------------------------------------------------------------------------------------------------- 
-- Mode 1, Stamp values are available -------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------- 
If @Cnt <> 0
	Begin
	 SELECT     
		CASE WHEN postlog_dt > maestro_dt THEN 'Contains data up to: ' + DATENAME(WEEKDAY, postlog_dt) + ', ' + CAST(postlog_dt AS varchar) 
			  ELSE 'Contains data up to: ' + DATENAME(WEEKDAY, maestro_dt) + ', ' + CAST(maestro_dt AS varchar) END AS Label
		FROM v_data_source
 	End 

--------------------------------------------------------------------------------------------------------------------- 
-- Mode 2, Stamp values are un-available ------------------------------------------------------------------------ 
--------------------------------------------------------------------------------------------------------------------- 
Else
  Begin
        Select 'Data Currency Cannot Be Determined - Please Contact IT' AS Label
        Return
  End
  
--------------------------------------------------------------------------------------------------------------------- 
--------------------------------------------------------------------------------------------------------------------- 
--------------------------------------------------------------------------------------------------------------------- 
END