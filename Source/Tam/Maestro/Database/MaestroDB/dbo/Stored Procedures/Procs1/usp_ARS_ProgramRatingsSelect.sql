CREATE PROCEDURE [dbo].[usp_ARS_ProgramRatingsSelect]
(
	@daypart_id int,
	@start_date datetime,
	@end_date datetime,
	@network_id int,
	@audience_id varchar(200),
	@program as varchar(99)
)
AS
BEGIN
	declare
		@period as varchar(4),
		@hh_audience_id as int,
		@nielson_network_id int,
		@media_month_id int;

	set @hh_audience_id = dbo.GetIDFromAudienceString( 'hh' );

	select @period = media_month, @media_month_id = id from media_months where @start_date between start_date and end_date;
	select	@nielson_network_id = id from nielsen_networks where nielsen_id in (select map_value from network_maps where map_set='Nielsen' and network_id=@network_id);

	PRINT 'NEILSON_ID: ' + cast(@nielson_network_id as varchar);
	PRINT 'DAYPART_ID: ' + cast(@daypart_id as varchar);
	PRINT 'STARTDATE: ' + cast(@start_date as varchar);
	PRINT 'ENDDATE: ' + cast(@end_date as varchar);
	PRINT 'NETWORK_ID: ' + cast(@network_id as varchar);
	PRINT 'AUDIENCE_ID: ' + cast(@audience_id as varchar);
	PRINT 'PROGRAM NAME: ' + @program;
	PRINT 'PERIOD: ' + @period;
	PRINT 'HH ID: ' + cast(@hh_audience_id as varchar);

	DECLARE @TempList table
	(
		AID int
	)

	DECLARE @TempAudienceID table
	(
		AID int
	)

	INSERT INTO @TempList (AID) VALUES (CAST(@hh_audience_id AS int));
	INSERT INTO @TempAudienceID (AID) VALUES (CAST(@hh_audience_id AS int));

	--BELOW WILL INSERT EVERY AUDIENCE ID WE NEED INTO A TEMP TABLE TO HELP FILTER THE QUERY
    --EARLY, TO SAVE TIME AND OVERHEAD
	DECLARE @AID varchar(10), @Pos int;

	SET @audience_id = LTRIM(RTRIM(@audience_id))+ ',';
	SET @Pos = CHARINDEX(',', @audience_id, 1);

	IF REPLACE(@audience_id, ',', '') <> ''
	BEGIN
		WHILE @Pos > 0
		BEGIN
			SET @AID = LTRIM(RTRIM(LEFT(@audience_id, @Pos - 1)));
			IF @AID <> ''
			BEGIN
				INSERT INTO @TempList (AID) (SELECT rating_audience_id FROM audience_audiences where custom_audience_id = @AID); --Use Appropriate conversion
				INSERT INTO @TempAudienceID (AID) VALUES (CAST(@AID AS int)); --Use Appropriate conversion
			
			END;
			SET @audience_id = RIGHT(@audience_id, LEN(@audience_id) - @Pos);
			SET @Pos = CHARINDEX(',', @audience_id, 1);

		END;
	END	;

	WITH monthly_ratings (
		daypart_id,
		nielsen_network_id,
		audience_id,
		tv_usage,
		audience_usage,
		universe
	)
	AS (
		select
			dp.id [daypart_id],
			mr.nielsen_network_id [nielsen_network_id],
			mpa.audience_id [audience_id], 
			avg( mta.usage ) [tv_usage],
			avg( mpa.usage ) [audience_usage],
			max( mua.universe ) [universe]
			from
			dbo.dayparts dp
			join dbo.timespans ts on ts.id = dp.timespan_id
			join dbo.daypart_days dp_d on dp.id = dp_d.daypart_id
			join dbo.days d on d.id = dp_d.day_id
			join dbo.mit_ratings mr on
				mr.media_month_id=@media_month_id
				and
				mr.rating_category_id=1
				and
				mr.nielsen_network_id = @nielson_network_id
				and
				mr.start_time between ts.start_time and ts.end_time
				and
				d.ordinal = datepart( weekday, mr.rating_date )
				and
				mr.rating_date between @start_date and @end_date
			join dbo.mit_person_audiences mpa on mpa.media_month_id=@media_month_id
				and mpa.mit_rating_id=mr.id
				and mpa.audience_id in (SELECT DISTINCT AID FROM @TempList)
			join dbo.mit_tv_audiences mta on mta.media_month_id=@media_month_id
				and mta.mit_rating_id=mr.id
				and mta.audience_id=mpa.audience_id
			join dbo.mit_universes mu on 
				mu.media_month_id=@media_month_id
				and
				mu.rating_category_id=1
				and
				mu.nielsen_network_id = @nielson_network_id
				and
				mr.rating_date between mu.start_date and mu.end_date
			join dbo.mit_universe_audiences mua on 
				mua.media_month_id=@media_month_id
				and
				mua.mit_universe_id=mu.id
				and
				mua.audience_id=mpa.audience_id
		where
			dp.id = @daypart_id
		group by
			dp.id, mr.nielsen_network_id, mpa.audience_id
	)

	SELECT
		@Period [Media Month],
		@Program [Program],
		@start_date [Start Date],
		@end_date [End Date],
		dp.name [Daypart],
		rtrim(nn.code) [Network],
		dbo.GetAudienceStringFromID( a_c.id ) [Audience],
		sum( mr.tv_usage ) / sum( mr.universe ) [HUT/PUT],
		sum( mr.audience_usage ) / sum( mr.universe ) [Rating],
		sum( mr.audience_usage ) / sum( mr.tv_usage ) [Share],
		sum( mr.tv_usage ) / 1000. [TVs (000)],
		sum( mr.audience_usage ) / 1000. [Viewers (000)],
		sum( mr.universe ) / 1000. [Universe (000)]
	FROM
		monthly_ratings mr
		join dbo.audience_audiences aa on mr.audience_id = aa.rating_audience_id
			and aa.rating_category_group_id=1
		join dbo.audiences a_c on a_c.id = aa.custom_audience_id
		join dbo.dayparts dp on dp.id = mr.daypart_id
		join dbo.nielsen_networks nn on nn.id = mr.nielsen_network_id
	WHERE 
		a_c.id in(SELECT AID from @TempAudienceID)
	GROUP BY
		dp.name,
		nn.code,
		a_c.id
	ORDER BY
		[Media Month],
		dp.name,
		nn.code,
		a_c.id;
END
