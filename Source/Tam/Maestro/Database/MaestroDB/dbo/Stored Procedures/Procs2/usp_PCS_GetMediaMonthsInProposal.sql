CREATE PROCEDURE [dbo].[usp_PCS_GetMediaMonthsInProposal]
(
    @start_date as datetime,
	@end_date as datetime       
)
AS
	SELECT distinct	
		id,year,month,media_month, start_date, end_date 
	FROM
		media_months (NOLOCK) 
	WHERE 
		(@start_date BETWEEN start_date AND end_date) OR (@end_date BETWEEN start_date AND end_date) OR
		(start_date BETWEEN @start_date AND @end_date) OR (end_date BETWEEN @start_date AND @end_date) 
	ORDER BY 
		start_date
