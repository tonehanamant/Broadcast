-- =============================================
-- Author:		jcarsley
-- Create date: 10/13/2010
-- Description:	This query is the data source for a report showing the number of records loaded for each week by category.
-- =============================================
CREATE proc [dbo].[usp_ARS_ReportRatingsWeekRecordsLoaded](
	 @start_date datetime
	,@end_date datetime
	,@rating_category_id int)
AS
BEGIN
	select 
		 [Media Week] = convert(varchar, media_weeks.start_date, 101) + ' - ' + convert(varchar, media_weeks.end_date, 101)
		,[Records Loaded] = count(*)
	from 
		nielsen_networks nn  WITH(NOLOCK)
		join mit_ratings mit WITH(NOLOCK)
			on nn.id = mit.nielsen_network_id
		join media_weeks WITH(NOLOCK)
			on mit.rating_date between media_weeks.start_date and media_weeks.end_date
	where 
		media_weeks.start_date <= @end_date
		and media_weeks.end_date >= @start_date
		and mit.rating_category_id = @rating_category_id
	group by
		media_weeks.start_date,
		 convert(varchar, media_weeks.start_date, 101) + ' - ' + convert(varchar, media_weeks.end_date, 101)
	order by  
		media_weeks.start_date desc
END
