-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/15/2014
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.udf_GetTrafficFlightText(38171)
-- SELECT dbo.udf_GetTrafficFlightText(38171)
CREATE FUNCTION udf_GetTrafficFlightText
(
	@traffic_id INT
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @return VARCHAR(MAX)
	
	DECLARE @start_date DATE;
	DECLARE @end_date DATE;
	DECLARE @selected BIT;
	
	DECLARE @num_flights INT;
	DECLARE @num_weeks_selected INT;
	DECLARE @start_run_date DATE;
	DECLARE @last_end_date DATE;
	DECLARE @is_last_week_selected BIT;

	DECLARE TrafficFlightCursor CURSOR FAST_FORWARD FOR
		SELECT
			tf.start_date,
			tf.end_date,
			tf.selected
		FROM
			traffic_flights tf (NOLOCK)
		WHERE
			tf.traffic_id=@traffic_id
		ORDER BY
			tf.start_date;

	SET @return = '';
	SET @num_weeks_selected = 0;
	SET @start_run_date = NULL;
	SET @num_flights = (SELECT COUNT(1) FROM traffic_flights tf (NOLOCK) WHERE tf.traffic_id=@traffic_id);
	SET @last_end_date = (SELECT MAX(tf.end_date) FROM traffic_flights tf (NOLOCK) WHERE tf.traffic_id=@traffic_id);
	SET @is_last_week_selected = (SELECT tf.selected FROM traffic_flights tf (NOLOCK) WHERE tf.traffic_id=@traffic_id AND tf.end_date=@last_end_date);
	
	OPEN TrafficFlightCursor
	FETCH NEXT FROM TrafficFlightCursor INTO @start_date,@end_date,@selected
	WHILE @@FETCH_STATUS = 0
		BEGIN
			IF @selected=1
				SET @num_weeks_selected = @num_weeks_selected + 1;
			IF @start_run_date IS NULL AND @selected=1
				SET @start_run_date = @start_date;
			ELSE IF @start_run_date IS NOT NULL AND @selected=0
				BEGIN
					SET @return = @return + CAST(DATEPART(month,@start_run_date) AS VARCHAR) + '/' + CAST(DATEPART(day,@start_run_date) AS VARCHAR) + ' - ' + CONVERT(VARCHAR, DATEADD(day,-1,@start_date), 101) + ', ';
					SET @start_run_date = NULL
				END
			FETCH NEXT FROM TrafficFlightCursor INTO @start_date,@end_date,@selected
		END
	CLOSE TrafficFlightCursor
	DEALLOCATE TrafficFlightCursor
	
	IF @num_flights > 0 AND @is_last_week_selected=1 AND @start_run_date IS NOT NULL
		SET @return = @return + CAST(DATEPART(month,@start_run_date) AS VARCHAR) + '/' + CAST(DATEPART(day,@start_run_date) AS VARCHAR) + ' - ' + CONVERT(VARCHAR, @last_end_date, 101) + ', ';

	IF LEN(@return) > 2
	BEGIN
		SET @return = SUBSTRING(@return, 0, LEN(@return));
		
		IF @num_weeks_selected <> @num_flights
			SET @return = @return + ' (' + CAST(@num_weeks_selected AS VARCHAR) + ' Over ' + CAST(@num_flights AS VARCHAR) + ' Week' + CASE WHEN @num_flights > 1 THEN 's' ELSE '' END + ')';
		ELSE
			SET @return = @return + ' (' + CAST(@num_flights AS VARCHAR) + ' Week' + CASE WHEN @num_flights > 1 THEN 's' ELSE '' END + ')';
	END
	ELSE
		SET @return = 'No Weeks Selected';
		
	RETURN @return;
END
