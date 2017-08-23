-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/26/2016
-- Description:	Get's the forward looking proposals from the current media month and on which are either ordered or on the avail planner.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalIdsForDailyImsCheck]
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    DECLARE @start_date DATE;
	SELECT
		@start_date = mm.start_date
	FROM
		media_months mm
	WHERE
		CONVERT(DATE,GETDATE()) BETWEEN mm.start_date AND mm.end_date
 
	SELECT 
		p.* 
	FROM 
		proposals p 
	WHERE 
		p.start_date>=@start_date 
		AND p.include_on_availability_planner=1
END