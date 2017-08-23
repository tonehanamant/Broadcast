
-- =============================================
-- Author:		jcarsley
-- Create date: 04/01/2011
-- Description:	Pivots the usp_RS_Weekly_GRP_TRP_Report procedure's data
-- USAGE: exec usp_RS_Pivot_Weekly_GRP_TRP_Report 29839, 0, 1, 1
-- =============================================
CREATE PROCEDURE [dbo].[usp_RS_Pivot_Weekly_GRP_TRP_Report]
	@proposal_id INT,
	@excel_total_post_delivery AS FLOAT,
	@rate_card_type VARCHAR(3),   -- GRP or TRP : -- 1=brand 2=direct response
	@audience_id INT              -- an audience_id from the proposal
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #tmp(
		proposal_id INT, 
		advertiser varchar(63),
		product varchar(127),
		agency varchar(63),
		contract_start datetime,
		contract_end datetime,
		total_weeks int,
		spot_length int,
		total_gross_cost money,
		daypart varchar(31),
		DMA varchar(63),
		rank int,
		[week] varchar(50),
		week_text varchar(50),
		rating_points float,
		version varchar(127))
					
	Insert  
	into #tmp
	exec usp_RS_Weekly_GRP_TRP_Report @proposal_id, @excel_total_post_delivery, @rate_card_type, @audience_id

	Select *
	from
	(
		Select DMA, rank, week, rating_points
		from #tmp
	) DataTable
	PIVOT
	(
	 Sum(rating_points)
	 FOR [week] in ([Wk 1],[Wk 2],[Wk 3],[Wk 4],[Wk 5])
	) PivotTable

END