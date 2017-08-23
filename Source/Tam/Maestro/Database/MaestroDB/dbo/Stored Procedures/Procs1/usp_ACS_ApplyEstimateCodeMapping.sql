-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/8/2011
-- Description:	This method does all checking and scrubbing internally.
-- =============================================
CREATE PROCEDURE usp_ACS_ApplyEstimateCodeMapping
	@product VARCHAR(255),
	@vendor_estimate_code VARCHAR(63),
	@start_date DATETIME,
	@end_date DATETIME,
	@tam_estimate_code VARCHAR(63),
	@estimate_id INT
AS
BEGIN
	DECLARE @does_mapping_already_exist INT
	DECLARE @media_month_id INT
	
	SET @does_mapping_already_exist = (
		SELECT COUNT(*) FROM estimate_code_maps (NOLOCK) WHERE 
			product=@product
			AND vendor_estimate_code=@vendor_estimate_code 
			AND start_date=@start_date 
			AND end_date=@end_date 
			AND tam_estimate_code=@tam_estimate_code
		)

	IF @does_mapping_already_exist = 0
	BEGIN
		-- insert mapping
		INSERT INTO estimate_code_maps (product, vendor_estimate_code, start_date, end_date, tam_estimate_code, estimate_id) VALUES (@product, @vendor_estimate_code, @start_date, @end_date, @tam_estimate_code, @estimate_id)
	END
END
