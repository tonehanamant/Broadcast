-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/24/2015
-- Description:	Get's a list of all the rule codes and media months that CAN be calculated. This is used by the RotationalBiasProcessor to determine what data to process.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_CanCrunchRotationalBiasCoefficients]
AS
BEGIN
	SET NOCOUNT ON;
	
	CREATE TABLE #potential_media_months (media_month_id INT)
	INSERT INTO #potential_media_months
		SELECT rbi.media_month_id FROM [mart].[rotational_bias_inputs] rbi (NOLOCK) WHERE rbi.media_month_id>399 GROUP BY rbi.media_month_id
		EXCEPT
		SELECT rbc.base_media_month_id FROM uvw_rotational_bias_coefficients_level_1 rbc (NOLOCK) WHERE rbc.base_media_month_id>399 GROUP BY rbc.base_media_month_id

	CREATE TABLE #output (rule_code TINYINT, media_month_id INT)

	DECLARE @media_month_id AS INT
	DECLARE MonthCursor CURSOR FAST_FORWARD FOR
		SELECT media_month_id FROM #potential_media_months

	OPEN MonthCursor
	FETCH NEXT FROM MonthCursor INTO @media_month_id
	WHILE @@FETCH_STATUS = 0
		BEGIN
			-- check rule code 0: static (if [mart].[rotational_bias_inputs] has all 3 needed months in it for rule code 0 then it can be processed)
			IF (SELECT COUNT(DISTINCT rbi.media_month_id) FROM [mart].[rotational_bias_inputs] rbi (NOLOCK) WHERE rbi.media_month_id IN (
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -1), 
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -2), 
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -3))) = 3
				INSERT INTO #output (rule_code, media_month_id) VALUES (0, @media_month_id);
			
			-- check rule code 1: seasonal (if [mart].[rotational_bias_inputs] has all 3 needed months in it for rule code 1 then it can be processed)
			IF (SELECT COUNT(DISTINCT rbi.media_month_id) FROM [mart].[rotational_bias_inputs] rbi (NOLOCK) WHERE rbi.media_month_id IN (
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -11), 
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -12), 
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -13))) = 3
				INSERT INTO #output (rule_code, media_month_id) VALUES (1, @media_month_id);

			-- check rule code 2: hybrid [static+seasonal] (if [mart].[rotational_bias_inputs] has all 6 needed months in it for rule code 2 then it can be processed)
			IF (SELECT COUNT(DISTINCT rbi.media_month_id) FROM [mart].[rotational_bias_inputs] rbi (NOLOCK) WHERE rbi.media_month_id IN (
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -1), 
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -2), 
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -3),
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -11), 
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -12), 
				dbo.udf_CalculateFutureMediaMonthId(@media_month_id, -13))) = 6
				INSERT INTO #output (rule_code, media_month_id) VALUES (2, @media_month_id);

			FETCH NEXT FROM MonthCursor INTO @media_month_id
		END
	CLOSE MonthCursor
	DEALLOCATE MonthCursor

	SELECT * FROM #output;
	
	DROP TABLE #potential_media_months;
	DROP TABLE #output;
END