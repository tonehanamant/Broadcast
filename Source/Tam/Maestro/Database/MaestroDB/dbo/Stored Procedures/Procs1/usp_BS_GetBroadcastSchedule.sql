CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastSchedule]
(
      @media_month_id int,
      @program_codes varchar(max),
      -- @duration float,
      @market_include_list varchar(max),
      @market_exclude_list varchar(max),
      @market_rank_include_list varchar(max),
      @market_rank_exclude_list varchar(max),
      @daypart_include_list varchar(max),
      @source_include_list varchar(max)
)
AS
BEGIN 
declare @query varchar(max);
      set @query = '';
      
      if(@media_month_id is null)
      BEGIN
      -- GET THE LATEST NOVEMBER MONTH WE HAVE LOADED IN THE SYSTEM!
      select @media_month_id = max(distinct mh.media_month_id) FROM external_rating.nsi.market_headers mh WITH (NOLOCK)
                  join media_months mm WITH (NOLOCK) on mm.id = mh.media_month_id
      where mm.month = 11
      END

      IF(@daypart_include_list IS NOT NULL)
      BEGIN
            set @query = @query + 'CREATE TABLE #daypart_temp(ID INT, MON SMALLINT, TUE SMALLINT, WED SMALLINT, THU SMALLINT, FRI SMALLINT, SAT SMALLINT, SUN SMALLINT, start_time int, end_time int); 
            INSERT INTO #daypart_temp(ID, MON, TUE, WED, THU, FRI, SAT, SUN, start_time, end_time) SELECT ID, MON, TUE, WED, THU, FRI, SAT, SUN, start_time, end_time FROM vw_ccc_daypart d WITH (NOLOCK) WHERE d.id in (' + @daypart_include_list + ');';
      END
      
set @query = @query + '
      SELECT DISTINCT
            pl.program_name, 
            dmap.map_value,
            pl.timezone, 
            pl.broadcast_channel_number, 
            pl.call_letters, 
            dh.primary_affiliation, 
            mm.start_date,
            mm.end_date,
            pl.program_duration,
            pl.market_code,
            pl.station_code,
            dh.call_letters,
            pl.program_code,
            pl.market_rank,
            pl.daypart_id
      from 
            external_rating.dbo.program_monthly_schedule pl WITH (NOLOCK)
            join media_months mm with (NOLOCK) on mm.id = pl.media_month_id 
            join vw_ccc_daypart d on d.id = pl.daypart_id
            join external_rating.nsi.distributor_headers dh WITH (NOLOCK) on dh.distributor_code = pl.station_code and dh.market_origin_code = pl.market_code
            join external_rating.nsi.market_headers mh WITH (NOLOCK) on mh.market_code = pl.market_code and pl.media_month_id = mh.media_month_id
            join dma_maps dmap WITH (NOLOCK) on dmap.dma_id = mh.dma_id and dmap.map_set = ''Strata''';

      IF(@daypart_include_list IS NOT NULL)
      BEGIN
            set @query = @query + ' JOIN #daypart_temp dpv on ';        
            set @query = @query + ' ((dpv.sun = 1 and d.sun = 1) OR (dpv.mon = 1 and d.mon = 1) OR (dpv.tue = 1 and d.tue = 1)  OR (dpv.wed = 1 and d.wed = 1)  ';
            set @query = @query + ' OR (dpv.thu = 1 and d.thu = 1) OR (dpv.fri = 1 and d.fri = 1) OR (dpv.sat = 1 and d.sat = 1)) ';
            -- set @query = @query + ' (DATEPART(HOUR, pl.program_start)*3600 + DATEPART(MINUTE,pl.program_start)*60 + DATEPART(SECOND,pl.program_start) <= dpv.end_time AND ';
                  -- set @query = @query + ' DATEPART(HOUR, pl.program_end)*3600 + DATEPART(MINUTE,pl.program_end)*60 + DATEPART(SECOND,pl.program_end) >= dpv.start_time) ';

            -- set @query = @query + ' d.end_time <= dpv.end_time AND ';
            -- set @query = @query + ' d.start_time >= dpv.start_time) ';

      END   
      
            
      set @query = @query + '
      WHERE
            pl.totaldays >= 3 and
            dh.distribution_source_type = ''B'' and
            pl.media_month_id = ' + cast(@media_month_id as varchar) + ' and
            pl.program_code in (' + @program_codes + ')';
            -- ' AND pl.program_duration = ' + cast(@duration as varchar);
      
      IF(@daypart_include_list IS NOT NULL)
            BEGIN
                    set @query = @query + ' AND dbo.GetIntersectingDaypartHours(d.start_time, d.end_time, dpv.start_time, dpv.end_time) > 0 ';
            END   
          
      IF(@market_rank_include_list IS NOT NULL)
      BEGIN
            set @query = @query + ' AND pl.market_rank <= ' + @market_rank_include_list;
      END
      
      IF(@market_rank_exclude_list IS NOT NULL)
      BEGIN
            set @query = @query + ' AND pl.market_rank >= ' + @market_rank_exclude_list;
      END
            
      IF @market_include_list IS NOT NULL
      BEGIN
            set @query = @query + ' AND pl.market_code in (' + @market_include_list + ')';
      END
      IF @market_exclude_list IS NOT NULL
      BEGIN
            set @query = @query + ' AND pl.market_code not in (' + @market_exclude_list + ')';  
      END
      IF @source_include_list IS NOT NULL
      BEGIN
            set @query = @query + ' AND pl.program_source in (' + @source_include_list + ')';      
      END
      
      set @query = @query + ' order by dmap.map_value, mm.start_date';
      set @query = @query + ';';

IF(@daypart_include_list IS NOT NULL)
BEGIN
      set @query = @query + 'DROP TABLE #daypart_temp;';
END

--print @query;
exec (@query);
            
END

