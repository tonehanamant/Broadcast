-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/26/2010
-- Description:	Checks whether or not a given air time is "in-spec" according to the zone and start/end times.
--				5 minute buffer for start/end time and 15 minute buffer for echo taken into account.
-- =============================================
CREATE FUNCTION [dbo].[udf_AirTimeInSpec]
(
	@zone_id INT,
	@air_time INT,
	@start_time INT,
	@end_time INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @return BIT
	SET @return = 0

	DECLARE @default_air_time_buffer INT
	DECLARE @echo_air_time_buffer INT
	DECLARE @echo_zone_id INT

	SET @echo_zone_id = 1421
	SET @default_air_time_buffer = 5  * 60
	SET @echo_air_time_buffer	 = 15 * 60

	DECLARE @adjusted_start_time INT
	DECLARE @adjusted_end_time INT
	
	-- adjust start and times by applying appropriate buffer in seconds
	SET @adjusted_start_time = (CASE WHEN @start_time - (CASE WHEN @zone_id = @echo_zone_id THEN @echo_air_time_buffer ELSE @default_air_time_buffer END) < 0 THEN 86400 - ABS(@start_time - (CASE WHEN @zone_id = @echo_zone_id THEN @echo_air_time_buffer ELSE @default_air_time_buffer END)) ELSE @start_time - (CASE WHEN @zone_id = @echo_zone_id THEN @echo_air_time_buffer ELSE @default_air_time_buffer END) END)
	SET @adjusted_end_time	 = (CASE WHEN @end_time + (CASE WHEN @zone_id = @echo_zone_id THEN @echo_air_time_buffer ELSE @default_air_time_buffer END) > 86400 THEN ABS(86400 - (@end_time + (CASE WHEN @zone_id = @echo_zone_id THEN @echo_air_time_buffer ELSE @default_air_time_buffer END))) ELSE @end_time + (CASE WHEN @zone_id = @echo_zone_id THEN @echo_air_time_buffer ELSE @default_air_time_buffer END) END)
	
	IF @adjusted_end_time < @adjusted_start_time
		BEGIN
			-- overnight time check
			IF (@air_time BETWEEN @adjusted_start_time AND 86400) OR (@air_time BETWEEN 0 AND @adjusted_end_time)
				SET @return = 1
		END
	ELSE
		BEGIN
			-- normal time check
			IF @air_time BETWEEN @adjusted_start_time AND @adjusted_end_time
				SET @return = 1
		END

	RETURN @return;
END
