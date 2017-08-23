CREATE PROCEDURE [dbo].[usp_NSI_CombineProgramLineupAndQuarterHourRatings]
	-- Add the parameters for the stored procedure here
	@code_month VARCHAR(5),
	@program_day_code INT,
    @time_start INT,
    @time_end INT
	
AS
BEGIN
	SET NOCOUNT ON;
	                            
	SELECT
		mm.[media_month] [Month],
		plu.[market_code] [Market Code],
		plu.[market_name] [Market Name],
		plu.[market_rank] [Market Rank],
		plu.[timezone] [Market Time Zone],
		plu.[station_code] [Station Code],
		plu.[call_letters] [Station Call Letters],
		plu.[broadcast_channel_number] [Broadcast Channel],
		plu.[network_affiliation] [Network Affiliation],
		CONVERT(VARCHAR, plu.[program_start], 120) [Program Start],
		CONVERT(VARCHAR, plu.[program_end], 120) [Program End],
		plu.[program_day_code] [Program Day Code],
		plu.[program_name] [Program Name],
		plu.[program_duration] [Program Duration],
		CONVERT(VARCHAR, qhr.[quarter_hour_date], 111) [Quarter Hour Date],
		qhr.[quarter_hour_day_code] [Quarter Hour Day Code],
		CONVERT(VARCHAR, DATEADD(SECOND, qhr.[quarter_hour_time], qhr.[quarter_hour_date]), 108) [Quarter Hour Start Time],
		qhr.[quarter_hour_code] [Qurter Hour Code],
		qhr.[tv_hh_ue] [TV Household Universe],
		qhr.[hut] [HUT],
		qhr.[delivery] * 0.001 [Household (000)s],
		CASE
			WHEN qhr.[tv_hh_ue] = 0 THEN NULL
			ELSE (qhr.[delivery] / qhr.[tv_hh_ue]) * 100.
		END [Quarter Hour Household Rating],
		CASE
			WHEN qhr.[hut] = 0 THEN NULL
			ELSE (qhr.[delivery] / qhr.[hut]) * 100.
		END [Quarter Hour Household Share]
	FROM
		media_months mm WITH(NOLOCK)
		JOIN program_lineup plu WITH(NOLOCK) ON
			mm.[id] = plu.[nsi_media_month_id]
		JOIN quarter_hour_ratings qhr WITH(NOLOCK) ON
			mm.[id] = qhr.[nsi_media_month_id]
			AND
			plu.[market_code] = qhr.[market_code]
			AND
			plu.[station_code] = qhr.[station_code]
			AND
			DATEADD(SECOND, qhr.[quarter_hour_time], qhr.[quarter_hour_date]) BETWEEN plu.[program_start] AND plu.[program_end]
	WHERE
		@code_month = mm.[media_month]
		AND
		@program_day_code = plu.[program_day_code]
		AND
		qhr.[quarter_hour_time] BETWEEN @time_start AND @time_end
	ORDER BY
		[Market Rank],
		[Station Call Letters],
		qhr.[quarter_hour_date],
		qhr.[quarter_hour_time];
	
END

