-- =============================================
-- Author:		Michael Norris
-- Create date: 03/30/2016
-- Description:	
-- =============================================
CREATE PROCEDURE dbo.usp_PCS_IsMultiDaypartProposal
	(@id int)
AS
BEGIN
	
	SET NOCOUNT ON;
	SELECT dbo.IsMultiDaypartPlan(@id)
END