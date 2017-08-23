-- =============================================
-- Author:		Michael Norris
-- Create date: 03/29/2016
-- Description:	Returns whether a plan is Multi Daypart excluding daypartnetworks.
-- =============================================
CREATE FUNCTION [dbo].[IsMultiDaypartPlan]
(
	@proposal_id INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @return BIT
	DECLARE @daypartids TABLE(id int)
	
	INSERT INTO @daypartids
	SELECT DISTINCT
			pd.daypart_id
		FROM 
			proposal_details pd (NOLOCK) 
		WHERE 
			pd.proposal_id=@proposal_id
		and pd.network_id not in (Select network_id from network_maps where map_set = 'daypartnetworks')
			

	declare @startcount int

	select @startcount = Count(*) from @daypartids
	
	IF @startcount > 1
		SET @return = 1;
	ELSE
		SET @return = 0;
	
	RETURN @return;
END