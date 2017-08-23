/* usage:  exec john_usp_ARS_GetMITRatingsLoaded 341, 1 */
CREATE PROCEDURE [dbo].[usp_ARS_GetMITRatingsLoaded]
(
	 @media_id int
	,@rating_category_id int
)
AS
BEGIN
	DECLARE @TempOutput table
	(
		week_number int,
		start_date datetime,
		end_date datetime,
		day1 int,
		day2 int,
		day3 int,
		day4 int,
		day5 int,
		day6 int,
		day7 int 
	)

	--INSERT ALL THE WEEKS THAT WE HAVE FOR THE MONTH and LOAD THE OUTPUT TABLE
	INSERT INTO @TempOutput (week_number, start_date, end_date) 
	(SELECT 
		mw.week_number, mw.start_date, mw.end_date 
	 from
		media_weeks mw 
			inner join media_months mm on mw.media_month_id = mm.id 
	 where mm.id = @media_id);

	--NOW, LETS LOOP THROUGH EACH MEDIA WEEK AND QUERY THE MIT_RATINGS TABLE TO SEE WHAT DAY WE ACTUALLY HAVE DATA LOADED FOR
	--REMEMBER, DATA MUST BE ON OR AFTER 6AM FOR IT TO COUNT.

	declare
		@week_number as int,
		@start_date as datetime,
		@ratings_count as int,
		@count as int,
		@tempbool as int;


	DECLARE temp_cursor CURSOR FOR 
	SELECT week_number, start_date FROM @TempOutput order by week_number;

	OPEN temp_cursor
	FETCH temp_cursor INTO @week_number, @start_date;
	WHILE @@fetch_status = 0
	BEGIN
		PRINT 'WEEK NUM: ' + cast(@week_number as varchar);
		PRINT 'START DATE: ' + cast(@start_date as varchar);

		SET @count = 1;

		WHILE (@count <= 7)
		BEGIN
			SELECT 
				@ratings_count = COUNT(mr.rating_date) 
			from 
				mit_ratings mr WITH(NOLOCK)
			where 
				mr.media_month_id=@media_id
				AND mr.rating_category_id = @rating_category_id
				AND (
					(mr.rating_date = @start_date AND mr.start_time >= 21600)
					OR 
					(mr.rating_date = dateadd(day, 1, @start_date) AND mr.start_time < 21600)
				)
				
			PRINT 'DATE IS: ' + cast(@start_date as varchar);
			PRINT 'RATINGS_COUNT: ' + cast(@ratings_count as varchar);

			--If we have a count > 0, that means we have ratings loaded for that date!!
			IF(@ratings_count > 0)
			BEGIN
				SET @tempbool = 1; 
			END;
			ELSE
			BEGIN
				SET @tempbool = 0;
			END;

			IF(@count = 1)
				UPDATE @TempOutput SET day1 = @tempbool WHERE week_number = @week_number;
			ELSE IF(@count = 2)
				UPDATE @TempOutput SET day2 = @tempbool WHERE week_number = @week_number;
			ELSE IF(@count = 3)
				UPDATE @TempOutput SET day3 = @tempbool WHERE week_number = @week_number;
			ELSE IF(@count = 4)
				UPDATE @TempOutput SET day4 = @tempbool WHERE week_number = @week_number;
			ELSE IF(@count = 5)
				UPDATE @TempOutput SET day5 = @tempbool WHERE week_number = @week_number;
			ELSE IF(@count = 6)
				UPDATE @TempOutput SET day6 = @tempbool WHERE week_number = @week_number;
			ELSE IF(@count = 7)
				UPDATE @TempOutput SET day7 = @tempbool WHERE week_number = @week_number;

			--INCREMENT THE DATE
			SET @count = @count + 1;

			--INCREMENT THE DAY BY ONE ALSO
			SET @start_date = dateadd(day, 1, @start_date);

		END;
		FETCH temp_cursor INTO @week_number, @start_date;
	END;

	DEALLOCATE temp_cursor;

	SELECT * FROM @TempOutput;
	RETURN;
	--DROP TABLE @TempOutput;
END
