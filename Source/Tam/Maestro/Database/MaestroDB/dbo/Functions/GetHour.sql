-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetHour
(
	@air_time INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT

	SET @return = (
		CASE
			WHEN @air_time BETWEEN 0	  AND 3599  THEN 1
			WHEN @air_time BETWEEN 3600  AND 7199  THEN 2
			WHEN @air_time BETWEEN 7200  AND 10799 THEN 3
			WHEN @air_time BETWEEN 10800 AND 14399 THEN 4
			WHEN @air_time BETWEEN 14400 AND 17999 THEN 5
			WHEN @air_time BETWEEN 18000 AND 21599 THEN 6
			WHEN @air_time BETWEEN 21600 AND 25199 THEN 7
			WHEN @air_time BETWEEN 25200 AND 28799 THEN 8
			WHEN @air_time BETWEEN 28800 AND 32399 THEN 9
			WHEN @air_time BETWEEN 32400 AND 35999 THEN 10
			WHEN @air_time BETWEEN 36000 AND 39599 THEN 11
			WHEN @air_time BETWEEN 39600 AND 43199 THEN 12
			WHEN @air_time BETWEEN 43200 AND 46799 THEN 13
			WHEN @air_time BETWEEN 46800 AND 50399 THEN 14
			WHEN @air_time BETWEEN 50400 AND 53999 THEN 15
			WHEN @air_time BETWEEN 54000 AND 57599 THEN 16
			WHEN @air_time BETWEEN 57600 AND 61199 THEN 17
			WHEN @air_time BETWEEN 61200 AND 64799 THEN 18
			WHEN @air_time BETWEEN 64800 AND 68399 THEN 19
			WHEN @air_time BETWEEN 68400 AND 71999 THEN 20
			WHEN @air_time BETWEEN 72000 AND 75599 THEN 21
			WHEN @air_time BETWEEN 75600 AND 79199 THEN 22
			WHEN @air_time BETWEEN 79200 AND 82799 THEN 23
			WHEN @air_time BETWEEN 82800 AND 86400 THEN 24
		END
	)

	RETURN @return
END
