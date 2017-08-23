-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/29/2011
-- Description:	
-- =============================================
CREATE FUNCTION [dbo].[IsOvernightPlan]
(
	@proposal_id INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @return BIT
	DECLARE @starttimes TABLE(starttime decimal, endtime decimal)
	
	INSERT INTO @starttimes
	SELECT DISTINCT
			((d.start_time / 60) / 60),
			((d.end_time / 60) / 60)
		FROM 
			proposal_details pd (NOLOCK) 
			join vw_ccc_daypart d on d.id = pd.daypart_id
		WHERE 
			pd.proposal_id=@proposal_id
			

	declare @startcount int

	select @startcount = Count(*) from @starttimes
	where starttime >= 6 or endtime > 6

	IF @startcount = 0
		SET @return = 1; --true
	ELSE
		SET @return = 0; --false
	
	RETURN @return;

END
