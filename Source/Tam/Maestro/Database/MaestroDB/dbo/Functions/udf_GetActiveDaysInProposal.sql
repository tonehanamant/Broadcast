-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION udf_GetActiveDaysInProposal
(	
	@media_month_id INT,
	@proposal_id INT
)
RETURNS @return TABLE
(
	date_of DATE
)
AS
BEGIN
	DECLARE @current_date DATE;
	DECLARE @start_date DATE;
	DECLARE @end_date DATE;

	SELECT
		@start_date = MIN(pf.start_date),
		@end_date = MAX(pf.end_date)
	FROM
		proposal_flights pf (NOLOCK)
	WHERE
		pf.proposal_id=@proposal_id;

	SET @current_date = @start_date;

	WHILE @current_date <= @end_date
	BEGIN
		IF	(SELECT COUNT(1) FROM proposal_flights pf (NOLOCK) WHERE pf.proposal_id=@proposal_id AND @current_date BETWEEN pf.start_date AND pf.end_date AND pf.selected=1) > 0
			AND
			(SELECT COUNT(1) FROM proposals p (NOLOCK) JOIN vw_ccc_daypart d ON d.id=p.primary_daypart_id WHERE p.id=@proposal_id AND 1=CASE WHEN datepart(weekday,@current_date)=1 AND d.mon=1 THEN 1 WHEN datepart(weekday,@current_date)=2 AND d.tue=1 THEN 1 WHEN datepart(weekday,@current_date)=3 AND d.wed=1 THEN 1 WHEN datepart(weekday,@current_date)=4 AND d.thu=1 THEN 1 WHEN datepart(weekday,@current_date)=5 AND d.fri=1 THEN 1 WHEN datepart(weekday,@current_date)=6 AND d.sat=1 THEN 1 WHEN datepart(weekday,@current_date)=7 AND d.sun=1 THEN 1 ELSE 0 END) > 0
			INSERT INTO @return SELECT @current_date
	
		SET @current_date = DATEADD(day,1,@current_date);		
	END
	RETURN;
END
