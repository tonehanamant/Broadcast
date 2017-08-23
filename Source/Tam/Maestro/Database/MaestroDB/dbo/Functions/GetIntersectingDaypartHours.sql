-- SELECT dbo.GetIntersectingDaypartHours(18000,68400,64800,21600)
CREATE FUNCTION [dbo].[GetIntersectingDaypartHours]
(
	@start_time1 INT,
	@end_time1 INT,
	@start_time2 INT,
	@end_time2 INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT
	
	DECLARE @h1 TINYINT
	DECLARE @h2 TINYINT
	DECLARE @h3 TINYINT
	DECLARE @h4 TINYINT
	DECLARE @h5 TINYINT
	DECLARE @h6 TINYINT
	DECLARE @h7 TINYINT
	DECLARE @h8 TINYINT
	DECLARE @h9 TINYINT
	DECLARE @h10 TINYINT
	DECLARE @h11 TINYINT
	DECLARE @h12 TINYINT
	DECLARE @h13 TINYINT
	DECLARE @h14 TINYINT
	DECLARE @h15 TINYINT
	DECLARE @h16 TINYINT
	DECLARE @h17 TINYINT
	DECLARE @h18 TINYINT
	DECLARE @h19 TINYINT
	DECLARE @h20 TINYINT
	DECLARE @h21 TINYINT
	DECLARE @h22 TINYINT
	DECLARE @h23 TINYINT
	DECLARE @h24 TINYINT

	SET @h1 = 0
	SET @h2 = 0
	SET @h3 = 0
	SET @h4 = 0
	SET @h5 = 0
	SET @h6 = 0
	SET @h7 = 0
	SET @h8 = 0
	SET @h9 = 0
	SET @h10 = 0
	SET @h11 = 0
	SET @h12 = 0
	SET @h13 = 0
	SET @h14 = 0
	SET @h15 = 0
	SET @h16 = 0
	SET @h17 = 0
	SET @h18 = 0
	SET @h19 = 0
	SET @h20 = 0
	SET @h21 = 0
	SET @h22 = 0
	SET @h23 = 0
	SET @h24 = 0

	SET @h1 =	CASE	WHEN @start_time1 < @end_time1 AND (0 BETWEEN @start_time1 AND @end_time1 OR 3599 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (3599 BETWEEN @start_time1 AND 86400 OR 3599 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (0 BETWEEN @start_time2 AND @end_time2 OR 3599 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (3599 BETWEEN @start_time2 AND 86400 OR 3599 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h2 =	CASE	WHEN @start_time1 < @end_time1 AND (3600 BETWEEN @start_time1 AND @end_time1 OR 7199 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (7199 BETWEEN @start_time1 AND 86400 OR 7199 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (3600 BETWEEN @start_time2 AND @end_time2 OR 7199 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (7199 BETWEEN @start_time2 AND 86400 OR 7199 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h3 =	CASE	WHEN @start_time1 < @end_time1 AND (7200 BETWEEN @start_time1 AND @end_time1 OR 10799 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (10799 BETWEEN @start_time1 AND 86400 OR 10799 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (7200 BETWEEN @start_time2 AND @end_time2 OR 10799 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (10799 BETWEEN @start_time2 AND 86400 OR 10799 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h4 =	CASE	WHEN @start_time1 < @end_time1 AND (10800 BETWEEN @start_time1 AND @end_time1 OR 14399 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (14399 BETWEEN @start_time1 AND 86400 OR 14399 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (10800 BETWEEN @start_time2 AND @end_time2 OR 14399 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (14399 BETWEEN @start_time2 AND 86400 OR 14399 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h5 =	CASE	WHEN @start_time1 < @end_time1 AND (14400 BETWEEN @start_time1 AND @end_time1 OR 17999 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (17999 BETWEEN @start_time1 AND 86400 OR 17999 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (14400 BETWEEN @start_time2 AND @end_time2 OR 17999 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (17999 BETWEEN @start_time2 AND 86400 OR 17999 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h6 =	CASE	WHEN @start_time1 < @end_time1 AND (18000 BETWEEN @start_time1 AND @end_time1 OR 21599 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (21599 BETWEEN @start_time1 AND 86400 OR 21599 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (18000 BETWEEN @start_time2 AND @end_time2 OR 21599 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (21599 BETWEEN @start_time2 AND 86400 OR 21599 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h7 =	CASE	WHEN @start_time1 < @end_time1 AND (21600 BETWEEN @start_time1 AND @end_time1 OR 25199 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (25199 BETWEEN @start_time1 AND 86400 OR 25199 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (21600 BETWEEN @start_time2 AND @end_time2 OR 25199 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (25199 BETWEEN @start_time2 AND 86400 OR 25199 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h8 =	CASE	WHEN @start_time1 < @end_time1 AND (25200 BETWEEN @start_time1 AND @end_time1 OR 28799 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (28799 BETWEEN @start_time1 AND 86400 OR 28799 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (25200 BETWEEN @start_time2 AND @end_time2 OR 28799 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (28799 BETWEEN @start_time2 AND 86400 OR 28799 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h9 =	CASE	WHEN @start_time1 < @end_time1 AND (28800 BETWEEN @start_time1 AND @end_time1 OR 32399 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (32399 BETWEEN @start_time1 AND 86400 OR 32399 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (28800 BETWEEN @start_time2 AND @end_time2 OR 32399 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (32399 BETWEEN @start_time2 AND 86400 OR 32399 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h10 =	CASE	WHEN @start_time1 < @end_time1 AND (32400 BETWEEN @start_time1 AND @end_time1 OR 35999 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (35999 BETWEEN @start_time1 AND 86400 OR 35999 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (32400 BETWEEN @start_time2 AND @end_time2 OR 35999 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (35999 BETWEEN @start_time2 AND 86400 OR 35999 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h11 =	CASE	WHEN @start_time1 < @end_time1 AND (36000 BETWEEN @start_time1 AND @end_time1 OR 39599 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (39599 BETWEEN @start_time1 AND 86400 OR 39599 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (36000 BETWEEN @start_time2 AND @end_time2 OR 39599 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (39599 BETWEEN @start_time2 AND 86400 OR 39599 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h12 =	CASE	WHEN @start_time1 < @end_time1 AND (39600 BETWEEN @start_time1 AND @end_time1 OR 43199 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (43199 BETWEEN @start_time1 AND 86400 OR 43199 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (39600 BETWEEN @start_time2 AND @end_time2 OR 43199 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (43199 BETWEEN @start_time2 AND 86400 OR 43199 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h13 =	CASE	WHEN @start_time1 < @end_time1 AND (43200 BETWEEN @start_time1 AND @end_time1 OR 46799 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (46799 BETWEEN @start_time1 AND 86400 OR 46799 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (43200 BETWEEN @start_time2 AND @end_time2 OR 46799 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (46799 BETWEEN @start_time2 AND 86400 OR 46799 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h14 =	CASE	WHEN @start_time1 < @end_time1 AND (46800 BETWEEN @start_time1 AND @end_time1 OR 50399 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (50399 BETWEEN @start_time1 AND 86400 OR 50399 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (46800 BETWEEN @start_time2 AND @end_time2 OR 50399 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (50399 BETWEEN @start_time2 AND 86400 OR 50399 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h15 =	CASE	WHEN @start_time1 < @end_time1 AND (50400 BETWEEN @start_time1 AND @end_time1 OR 53999 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (53999 BETWEEN @start_time1 AND 86400 OR 53999 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (50400 BETWEEN @start_time2 AND @end_time2 OR 53999 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (53999 BETWEEN @start_time2 AND 86400 OR 53999 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h16 =	CASE	WHEN @start_time1 < @end_time1 AND (54000 BETWEEN @start_time1 AND @end_time1 OR 57599 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (57599 BETWEEN @start_time1 AND 86400 OR 57599 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (54000 BETWEEN @start_time2 AND @end_time2 OR 57599 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (57599 BETWEEN @start_time2 AND 86400 OR 57599 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h17 =	CASE	WHEN @start_time1 < @end_time1 AND (57600 BETWEEN @start_time1 AND @end_time1 OR 61199 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (61199 BETWEEN @start_time1 AND 86400 OR 61199 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (57600 BETWEEN @start_time2 AND @end_time2 OR 61199 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (61199 BETWEEN @start_time2 AND 86400 OR 61199 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h18 =	CASE	WHEN @start_time1 < @end_time1 AND (61200 BETWEEN @start_time1 AND @end_time1 OR 64799 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (64799 BETWEEN @start_time1 AND 86400 OR 64799 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (61200 BETWEEN @start_time2 AND @end_time2 OR 64799 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (64799 BETWEEN @start_time2 AND 86400 OR 64799 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h19 =	CASE	WHEN @start_time1 < @end_time1 AND (64800 BETWEEN @start_time1 AND @end_time1 OR 68399 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (68399 BETWEEN @start_time1 AND 86400 OR 68399 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (64800 BETWEEN @start_time2 AND @end_time2 OR 68399 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (68399 BETWEEN @start_time2 AND 86400 OR 68399 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h20 =	CASE	WHEN @start_time1 < @end_time1 AND (68400 BETWEEN @start_time1 AND @end_time1 OR 71999 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (71999 BETWEEN @start_time1 AND 86400 OR 71999 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (68400 BETWEEN @start_time2 AND @end_time2 OR 71999 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (71999 BETWEEN @start_time2 AND 86400 OR 71999 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h21 =	CASE	WHEN @start_time1 < @end_time1 AND (72000 BETWEEN @start_time1 AND @end_time1 OR 75599 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (75599 BETWEEN @start_time1 AND 86400 OR 75599 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (72000 BETWEEN @start_time2 AND @end_time2 OR 75599 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (75599 BETWEEN @start_time2 AND 86400 OR 75599 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h22 =	CASE	WHEN @start_time1 < @end_time1 AND (75600 BETWEEN @start_time1 AND @end_time1 OR 79199 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (79199 BETWEEN @start_time1 AND 86400 OR 79199 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (75600 BETWEEN @start_time2 AND @end_time2 OR 79199 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (79199 BETWEEN @start_time2 AND 86400 OR 79199 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h23 =	CASE	WHEN @start_time1 < @end_time1 AND (79200 BETWEEN @start_time1 AND @end_time1 OR 82799 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (82799 BETWEEN @start_time1 AND 86400 OR 82799 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (79200 BETWEEN @start_time2 AND @end_time2 OR 82799 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (82799 BETWEEN @start_time2 AND 86400 OR 82799 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @h24 =	CASE	WHEN @start_time1 < @end_time1 AND (82800 BETWEEN @start_time1 AND @end_time1 OR 86399 BETWEEN @start_time1 AND @end_time1) THEN 1
						WHEN @end_time1 < @start_time1 AND (86399 BETWEEN @start_time1 AND 86400 OR 86399 BETWEEN 0 AND @end_time1) THEN 1 ELSE 0 END
						&
				CASE	WHEN @start_time2 < @end_time2 AND (82800 BETWEEN @start_time2 AND @end_time2 OR 86399 BETWEEN @start_time2 AND @end_time2) THEN 1
						WHEN @end_time2 < @start_time2 AND (86399 BETWEEN @start_time2 AND 86400 OR 86399 BETWEEN 0 AND @end_time2) THEN 1 ELSE 0 END

	SET @return = @h1+@h2+@h3+@h4+@h5+@h6+@h7+@h8+@h9+@h10+@h11+@h12+@h13+@h14+@h15+@h16+@h17+@h18+@h19+@h20+@h21+@h22+@h23+@h24
	
	RETURN @return
END
