

CREATE PROCEDURE [dbo].[usp_REC_GetPassedMediaMonthsForYear]

AS

DECLARE @effective_date DATETIME
SET @effective_date = getdate()

select id,
		year,
		 month,
		 media_month,
		 start_date,
		 end_date 
	from media_months (NOLOCK) 
	where 
	media_months.year = DATEPART(year,@effective_date) 
	and 
	media_months.month <= DATEPART(month,@effective_date)


