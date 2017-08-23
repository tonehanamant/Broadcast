-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/3/2015
-- Description:	Returns the 16 component dayparts used by the inventory management system. 8 M-F, 8 SA-SU in 3 hour increments
-- =============================================
CREATE PROCEDURE [dbo].[usp_ICS_GetInventoryComponentDayparts]
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	
	SELECT * FROM dbo.GetInventoryComponentDayparts()
END