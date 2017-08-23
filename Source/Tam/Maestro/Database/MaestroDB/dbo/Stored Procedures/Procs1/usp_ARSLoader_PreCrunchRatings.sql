CREATE PROCEDURE [dbo].[usp_ARSLoader_PreCrunchRatings]
	@codeMediaMonth as varchar(15),
	@codeRatingCategory as varchar(15)
AS
BEGIN

SET DATEFIRST 1

declare
	@idMediaMonth as int,
	@idRatingCategory as int;

BEGIN TRY
	BEGIN TRANSACTION;
--	if @codeRatingCategory <> 'NHIMIT' 
--		begin
--		raiserror(
--			'Rating category must be NHIMIT--not %s.', 
--			15, 
--			1,
--			@codeRatingCategory);
--		end
	select @idMediaMonth = id from media_months (NOLOCK) where media_month = @codeMediaMonth;
	if @idMediaMonth is NULL 
		begin
		raiserror(
			'Month, %s, is not in the media_months table.', 
			15, 
			1,
			@codeMediaMonth);
		end
	select @idRatingCategory = id from rating_categories (NOLOCK) where code = @codeRatingCategory;
	if @idRatingCategory is NULL 
		begin
		raiserror(
			'Rating category, %s, is not in the rating_categories table.', 
			15, 
			1,
			@codeRatingCategory);
		end

	declare @dayparts Table(
		daypart_id int
	)

	INSERT INTO @dayparts(daypart_id)
		SELECT DISTINCT
			daypart_id
		FROM 
			daypart_maps dpm (NOLOCK)
		WHERE
			dpm.map_set IN ('Ratings', 'FM_nDyptNum');

	declare @nielsen_networks Table(
		nielsen_network_id int
	);

	insert into @nielsen_networks(nielsen_network_id)
		select
			id
		from 
			nielsen_networks nn (NOLOCK);
	--	where
	--		nn.code in('adsm', 'nan', 'nick', 'toon');

	delete from 
		ratings 
	where 
		rating_category_id = @idRatingCategory
		AND base_media_month_id=@idMediaMonth;

	insert into ratings (
		rating_category_id,
		base_media_month_id,
		forecast_media_month_id,
		nielsen_network_id,	
		audience_id,
		daypart_id,
		audience_usage,
		tv_usage
	)
			select	
				[rating_category_id],
				[media_month_id] [base_media_month_id],
				[media_month_id] [forecast_media_month_id],
				[nielsen_network_id],
				[audience_id],
				[daypart_id],				
				avg( [audience_usage] ) [audience_usage],
				avg( [tv_usage] ) [tv_usage]
			from
			(
				select
					mr.rating_category_id [rating_category_id],
					mr.media_month_id,
					mr.nielsen_network_id,
					mrpa.audience_id [audience_id],
					dp.id [daypart_id],
					mrpa.usage [audience_usage],
					mrta.usage [tv_usage]
				from
					dayparts dp (NOLOCK)
					join timespans ts (NOLOCK) on ts.id = dp.timespan_id
					join daypart_days dp_d (NOLOCK) on dp.id = dp_d.daypart_id
					join days d (NOLOCK) on d.id = dp_d.day_id
					join @dayparts dpt on dpt.daypart_id = dp.id
					join mit_ratings mr (NOLOCK) on 
						mr.rating_category_id = @idRatingCategory
						AND mr.media_month_id=@idMediaMonth
						AND mr.nielsen_network_id in (
							select nielsen_network_id from @nielsen_networks
						)
						AND mr.start_time between ts.start_time and ts.end_time
						AND d.ordinal = datepart( weekday, mr.rating_date )
					join mit_tv_audiences mrta (NOLOCK) on 
						mrta.media_month_id=@idMediaMonth
						AND mrta.mit_rating_id=mr.id
					join mit_person_audiences mrpa (NOLOCK) on 
						mrpa.media_month_id=@idMediaMonth
						AND mrpa.mit_rating_id=mr.id
						AND mrta.audience_id = mrpa.audience_id
				where
					ts.start_time <= ts.end_time					

				union

				select
					mr.rating_category_id [rating_category_id],
					mr.media_month_id,
					mr.nielsen_network_id,
					mrpa.audience_id [audience_id],
					dp.id [daypart_id],
					mrpa.usage [audience_usage],
					mrta.usage [tv_usage]
				from
					dayparts dp (NOLOCK)
					join timespans ts (NOLOCK) on ts.id = dp.timespan_id
					join daypart_days dp_d (NOLOCK) on dp.id = dp_d.daypart_id
					join days d (NOLOCK) on d.id = dp_d.day_id
					join @dayparts dpt on dpt.daypart_id = dp.id
					join mit_ratings mr (NOLOCK) on mr.media_month_id=@idMediaMonth
						and mr.rating_category_id = @idRatingCategory
						and mr.nielsen_network_id in (
							select nielsen_network_id from @nielsen_networks
						)
						and
						(
							mr.start_time between ts.start_time and 86340
							or
							mr.start_time between 0 and ts.end_time
						)
						and d.ordinal = datepart( weekday, mr.rating_date )
					join mit_tv_audiences mrta (NOLOCK) on 
						mrta.media_month_id=@idMediaMonth
						and mrta.mit_rating_id=mr.id
					join mit_person_audiences mrpa (NOLOCK) on 
						mrpa.media_month_id=@idMediaMonth
						and mrpa.mit_rating_id=mr.id
						and mrta.audience_id = mrpa.audience_id
				where
					ts.start_time > ts.end_time					
			) as tmp
			group by
				[rating_category_id],
				[media_month_id],
				[nielsen_network_id],
				[audience_id],
				[daypart_id]
			order by
				[rating_category_id],
				[media_month_id],
				[nielsen_network_id],
				[audience_id];
	COMMIT TRANSACTION;   
END TRY
BEGIN CATCH
    DECLARE @ErrorNumber INT;
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;
    DECLARE @ErrorProcedure NVARCHAR(200) ;
    DECLARE @ErrorLine INT;
    DECLARE @ErrorMessage NVARCHAR(4000);
	SELECT 
		@ErrorNumber = ERROR_NUMBER(),
		@ErrorSeverity = ERROR_SEVERITY(),
		@ErrorState = ERROR_STATE(),
		@ErrorProcedure = isnull(ERROR_PROCEDURE(), 'N/A'),
		@ErrorLine = ERROR_LINE(),
		@ErrorMessage = N'Error %d, Level %d, State %d, Procedure %s, Line %d, ' + 
            'Message: '+ ERROR_MESSAGE();
	
	IF (XACT_STATE()) = -1-- If -1, the transaction is uncommittable
		BEGIN
		PRINT
			N'The transaction is in an uncommittable state. ' +
			'Rolling back transaction.'
		ROLLBACK TRANSACTION;
		END;
	ELSE IF (XACT_STATE()) = 1-- If 1, the transaction is committable.
		BEGIN
		PRINT
			N'The transaction is committable. ' +
			'Committing transaction.'
		COMMIT TRANSACTION;   
		END;

	RAISERROR(
		@ErrorMessage, 
		@ErrorSeverity, 
		1,               
		@ErrorNumber,    -- parameter: original error number.
		@ErrorSeverity,  -- parameter: original error severity.
		@ErrorState,     -- parameter: original error state.
		@ErrorProcedure, -- parameter: original error procedure name.
		@ErrorLine       -- parameter: original error line number.
	);
END CATCH;
END
