-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/15/2010
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetDaypartText]
(
	@mon BIT,
	@tue BIT,
	@wed BIT,
	@thu BIT,
	@fri BIT,
	@sat BIT,
	@sun BIT,
	@start_time AS INT,
	@end_time AS INT
)
RETURNS VARCHAR(31)
AS
BEGIN
	DECLARE @return VARCHAR(31)
	SET @return = '';

	DECLARE @start_day INT
	DECLARE @end_day INT
	DECLARE @current_day INT
	DECLARE @start_hour INT
	DECLARE @start_minute INT
	DECLARE @start_second INT
	DECLARE @end_hour INT
	DECLARE @end_minute INT
	DECLARE @end_second INT

	SET @start_day = 0;
	SET @end_day = 0;
	SET @current_day = 0;
	SET @start_hour = 0;
	SET @start_minute = 0;
	SET @start_second = 0;
	SET @end_hour = 0;
	SET @end_minute = 0;
	SET @end_second = 0;

	WHILE @current_day <= 6
		BEGIN
			
			IF (CASE @current_day WHEN 0 THEN @mon WHEN 1 THEN @tue WHEN 2 THEN @wed WHEN 3 THEN @thu WHEN 4 THEN @fri WHEN 5 THEN @sat WHEN 6 THEN @sun END) = 1
				BEGIN
					SET @start_day = @current_day;
					SET @current_day = @current_day + 1;

					WHILE (@current_day<=6) AND ((CASE @current_day WHEN 0 THEN @mon WHEN 1 THEN @tue WHEN 2 THEN @wed WHEN 3 THEN @thu WHEN 4 THEN @fri WHEN 5 THEN @sat WHEN 6 THEN @sun END) = 1)
						BEGIN 
							SET @end_day = @current_day;
							SET @current_day = @current_day + 1;
						END

					IF @end_day > @start_day
						BEGIN
							SET @return =	@return + 
											(CASE WHEN LEN(@return)>0 THEN ',' ELSE '' END) + 
											(CASE @start_day WHEN 0 THEN 'M' WHEN 1 THEN 'TU' WHEN 2 THEN 'W' WHEN 3 THEN 'TH' WHEN 4 THEN 'F' WHEN 5 THEN 'SA' WHEN 6 THEN 'SU' END) + 
											'-' + 
											(CASE @end_day WHEN 0 THEN 'M' WHEN 1 THEN 'TU' WHEN 2 THEN 'W' WHEN 3 THEN 'TH' WHEN 4 THEN 'F' WHEN 5 THEN 'SA' WHEN 6 THEN 'SU' END);
						END
					ELSE
						BEGIN
							SET @return =	@return + 
											(CASE WHEN LEN(@return)>0 THEN ',' ELSE '' END) + 
											(CASE @start_day WHEN 0 THEN 'M' WHEN 1 THEN 'TU' WHEN 2 THEN 'W' WHEN 3 THEN 'TH' WHEN 4 THEN 'F' WHEN 5 THEN 'SA' WHEN 6 THEN 'SU' END);
						END
				END
			ELSE
				BEGIN
					SET @current_day = @current_day + 1;
				END								
		END

	SET @start_hour =		@start_time / 3600;
	SET @start_minute =		(@start_time % 3600) / 60;
	SET @start_second =		@start_time - ((@start_hour * 3600) + (@start_minute * 60));

	SET @end_hour =			@end_time / 3600;
	SET @end_minute =		(@end_time % 3600) / 60;
	SET @end_second =		@end_time - ((@end_hour * 3600) + (@end_minute * 60));

	IF @end_second = 59
		SET @end_second = @end_second + 1

	IF @end_second = 60
		BEGIN
			IF @end_minute = 59
				BEGIN
					SET @end_minute = 0
					SET @end_hour = @end_hour + 1
				END
			ELSE
				SET @end_minute = @end_minute + 1

			SET @end_second = 0
		END

	SET @return = @return + ' ';

	IF (@end_hour - @start_hour) = 24
		SET @return = @return + '24HR';
	ELSE
		BEGIN
			IF @start_minute = 0
				SET @return = @return + CAST((CASE WHEN @start_hour = 0 THEN '12' WHEN @start_hour > 12 THEN @start_hour - 12 ELSE @start_hour END) AS VARCHAR(2));
			ELSE
				SET @return = @return + CAST((CASE WHEN @start_hour = 0 THEN '12' WHEN @start_hour > 12 THEN @start_hour - 12 ELSE @start_hour END) AS VARCHAR(2)) + ':' + CAST(@start_minute AS VARCHAR(2));

			SET @return = @return + (CASE WHEN @start_hour = 0 THEN 'AM' WHEN @start_hour >= 12 THEN 'PM' ELSE 'AM' END) + '-';

			IF @end_minute = 0
				SET @return = @return + CAST((CASE WHEN @end_hour > 12 THEN @end_hour - 12 ELSE CASE WHEN @end_hour = 0 THEN 12 ELSE @end_hour END END) AS VARCHAR(2));
			ELSE
				SET @return = @return + CAST((CASE WHEN @end_hour > 12 THEN @end_hour - 12 ELSE CASE WHEN @end_hour = 0 THEN 12 ELSE @end_hour END END) AS VARCHAR(2)) + ':' + CAST(@end_minute AS VARCHAR(2));

			SET @return = @return + (CASE WHEN @end_hour = 0 OR @end_hour = 24 THEN 'AM' WHEN @end_hour >= 12 THEN 'PM' ELSE 'AM' END);
		END

	RETURN @return;
END
