-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_LookupDaypart
	@start_time INT,
	@end_time INT,
	@mon BIT,
	@tue BIT,
	@wed BIT,
	@thu BIT,
	@fri BIT,
	@sat BIT,
	@sun BIT
AS
BEGIN
	SELECT
		dp.id,
		dp.code,
		dp.name,
		dp.tier,
		ts.start_time,
		ts.end_time,
		CASE (sum(power(2,d.ordinal - 1)) & 0x01) 
			WHEN 0x01 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END [mon],
		CASE (sum(power(2,d.ordinal - 1)) & 0x02) 
			WHEN 0x02 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  [tue],
		CASE (sum(power(2,d.ordinal - 1)) & 0x04) 
			WHEN 0x04 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  [wed],
		CASE (sum(power(2,d.ordinal - 1)) & 0x08) 
			WHEN 0x08 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  [thu],
		CASE (sum(power(2,d.ordinal - 1)) & 0x10) 
			WHEN 0x10 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  [fri],
		CASE (sum(power(2,d.ordinal - 1)) & 0x20) 
			WHEN 0x20 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  [sat],
		CASE (sum(power(2,d.ordinal - 1)) & 0x40) 
			WHEN 0x40 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END [sun],
		dp.daypart_text,
		dp.total_hours
	FROM
		dayparts dp
		JOIN		timespans ts		ON ts.id = dp.timespan_id
		LEFT JOIN	daypart_days dp_d	ON dp.id = dp_d.daypart_id
		LEFT JOIN	days d				ON d.id  = dp_d.day_id
	WHERE
		start_time = @start_time
		AND end_time = @end_time
	GROUP BY
		dp.id,
		dp.code,
		dp.name,
		dp.tier,
		ts.start_time,
		ts.end_time,
		dp.daypart_text,
		dp.total_hours
	HAVING
		CASE (sum(power(2,d.ordinal - 1)) & 0x01) 
			WHEN 0x01 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END = @mon
		AND
		CASE (sum(power(2,d.ordinal - 1)) & 0x02) 
			WHEN 0x02 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END = @tue
		AND
		CASE (sum(power(2,d.ordinal - 1)) & 0x04) 
			WHEN 0x04 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END = @wed
		AND
		CASE (sum(power(2,d.ordinal - 1)) & 0x08) 
			WHEN 0x08 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END = @thu
		AND
		CASE (sum(power(2,d.ordinal - 1)) & 0x10) 
			WHEN 0x10 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END = @fri
		AND
		CASE (sum(power(2,d.ordinal - 1)) & 0x20) 
			WHEN 0x20 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END = @sat
		AND
		CASE (sum(power(2,d.ordinal - 1)) & 0x40) 
			WHEN 0x40 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END = @sun
END
