/************************ END: HOT FIX - DAYPART NETWORKS  **************************************************************/

CREATE FUNCTION [dbo].[FloatToMoney] (@value float)
RETURNS MONEY
AS
BEGIN
	RETURN CAST(@value as MONEY)
END