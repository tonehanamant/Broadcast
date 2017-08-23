-- =============================================
-- Author:		CRUD Creator
-- Create date: 01/16/2014 02:17:56 PM
-- Description:	Auto-generated method to insert a network_rate_card_daypart_adjustments record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_network_rate_card_daypart_adjustments_insert]
	@id INT OUTPUT,
	@sales_model_id INT,
	@daypart_id INT,
	@network_id INT,
	@start_date DATE,
	@end_date DATE,
	@weight FLOAT
AS
BEGIN
	INSERT INTO [dbo].[network_rate_card_daypart_adjustments]
	(
		[sales_model_id],
		[daypart_id],
		[network_id],
		[start_date],
		[end_date],
		[weight]
	)
	VALUES
	(
		@sales_model_id,
		@daypart_id,
		@network_id,
		@start_date,
		@end_date,
		@weight
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
