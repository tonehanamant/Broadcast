CREATE PROCEDURE [dbo].[usp_ARS_GetRatingsAdjustment]
	@effective_date DATETIME
AS
BEGIN
	SELECT CAST(value AS FLOAT) FROM uvw_properties p WHERE 
		name='prop_rating_adj'
		AND (p.start_date<=@effective_date AND (p.end_date>=@effective_date OR p.end_date IS NULL))
END
