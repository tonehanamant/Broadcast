
-- =============================================
-- Author:		John Carsley
-- Create date: 03/07/2013
-- =============================================
CREATE PROCEDURE [dbo].[usp_REL2_GetDisplayDayparts]
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --same as NOLOCK
	SET NOCOUNT ON;

	SELECT 
		id,
		code,
		name,
		start_time,
		end_time,
		mon,
		tue,
		wed,
		thu,
		fri,
		sat,
		sun 
	FROM 
		vw_ccc_daypart (NOLOCK) 
END
