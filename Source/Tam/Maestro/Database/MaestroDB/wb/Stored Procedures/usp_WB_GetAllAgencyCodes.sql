-- =============================================
-- Author: Mike Deaven
-- Create date: 03/12/2012
-- Description: Gets All Agency Codes
-- =============================================
CREATE PROCEDURE [wb].[usp_WB_GetAllAgencyCodes]
AS
BEGIN
select 
wba.code
from 
wb.wb_agencies wba with (NOLOCK) 
END