CREATE PROCEDURE [dbo].[usp_PCS_GetMediaWeekIDForStartDate]
(
      @start_date datetime	
)
AS

BEGIN
	Select 
		id
    from 
        media_weeks WITH (NOLOCK)
    where 
        @start_date between media_weeks.start_date and media_weeks.end_date
END
