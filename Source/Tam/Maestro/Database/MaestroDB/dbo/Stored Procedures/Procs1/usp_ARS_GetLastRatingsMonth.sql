CREATE PROCEDURE [dbo].[usp_ARS_GetLastRatingsMonth]
AS
BEGIN
	declare
		@max_end_date as datetime,
		@media_week_id as int,
		@media_month_year as int,
		@media_month_month as int,
		@media_month as varchar(15),
		@media_week_week_number as int,
		@media_month_id as int,
		@media_week_end_date as datetime,
		@date_count_1 as int,
		@date_count_2 as int;

	--Find the last end_date for the last week loaded into the database	
	SELECT @max_end_date = MAX(end_date) from mit_universes;

	PRINT 'END_DATE: ' + cast(@max_end_date as varchar);

	--This gives us the base month to start from
	SELECT @media_week_id = mw.id, @media_month_year = mm.year, @media_month_month = mm.month, 
	@media_month = mm.media_month, @media_week_week_number = mw.week_number, 
	@media_month_id = mw.media_month_id, @media_week_end_date = mw.end_date from media_weeks mw 
	inner join media_months mm on mw.media_month_id = mm.id 
	where @max_end_date between mw.start_date and mw.end_date;

	PRINT 'mw_id: ' + cast(@media_week_id as varchar);
	PRINT 'mm_year: ' + cast(@media_month_year as varchar);
	PRINT 'mm_month: ' + cast(@media_month_month as varchar);
	PRINT 'media_month: ' + @media_month;
	PRINT 'mw_week_number: ' + cast(@media_week_week_number as varchar);
	PRINT 'mm_id: ' + cast(@media_month_id as varchar);
	PRINT 'mw_end_date: ' + cast(@media_week_end_date as varchar);

	IF (@media_week_end_date = @max_end_date)
	BEGIN
		--CHECK TO SEE WE HAVE DATA FOR ALL DAYS!!!
		
		--Find out how many days are in the media month we are interested in
		SELECT @date_count_1 = COUNT(*) * 7 FROM media_weeks where media_month_id = @media_month_id;
		
		--Check to see if we have a match, if we do, data is loaded for the month
		SELECT @date_count_2 = count(distinct rating_date) from mit_ratings mr, media_months mm
		where mr.rating_date between mm.start_date and mm.end_date
		and mm.id = @media_month_id;
		
		IF(@date_count_1 = @date_count_2)
		BEGIN
			SELECT @media_month [media_month], @media_month_id[media_month_id];
			RETURN;
		END;
		ELSE
		BEGIN
			--TODO: FINISH THIS
			GOTO STARTLOOP;		
		END;

	END;
	--BELOW MEANS WE DID NOT HAVE A COMPLETE MONTH ABOVE, SO WE HAVE TO CHECK THE NEXT ONE DOWN
	ELSE
	BEGIN
		--LOOP UNTIL WE FIND THE RIGHT MONTH!!!
	STARTLOOP:	
		WHILE(1 = 1)
		BEGIN
			IF(@media_month_month <> 1)
			BEGIN
				SET @media_month_month = @media_month_month - 1;
			END;
			ELSE
			BEGIN
				SET @media_month_month = 12;
				SET @media_month_year = @media_month_year - 1;
			END;
			
			--GET THE MEDIA_MONTH ID For the next media month to check and do it again
			SELECT @media_month_id = id, @media_month = media_month FROM media_months where year = @media_month_year and month = @media_month_month;
			
			--Find out how many days are in the media month we are interested in
			SELECT @date_count_1 = COUNT(*) * 7 FROM media_weeks where media_month_id = @media_month_id;
				
			--Check to see if we have a match, if we do, data is loaded for the month
			SELECT 
				@date_count_2 = count(distinct rating_date) 
			from 
				mit_ratings mr (NOLOCK)
			where 
				mr.media_month_id=@media_month_month
				
			IF(@date_count_1 = @date_count_2)
			BEGIN
				SELECT @media_month [media_month], @media_month_id[media_month_id];
				RETURN;
			END;
		END;
	END;
END
