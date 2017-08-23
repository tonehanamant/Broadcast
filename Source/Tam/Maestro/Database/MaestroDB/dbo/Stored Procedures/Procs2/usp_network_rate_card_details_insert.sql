

-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/07/2014 09:39:36 AM
-- Description:	Auto-generated method to insert a network_rate_card_details record.
-- =============================================
CREATE PROCEDURE usp_network_rate_card_details_insert
	@id INT OUTPUT,
	@network_rate_card_id INT,
	@tier INT,
	@network_id INT,
	@primary_audience_id INT,
	@hh_us_universe FLOAT,
	@hh_coverage_universe FLOAT,
	@hh_cpm MONEY,
	@hh_rating FLOAT,
	@hh_delivery FLOAT,
	@hh_net_cost_cpm MONEY,
	@demo_us_universe FLOAT,
	@demo_coverage_universe FLOAT,
	@demo_cpm MONEY,
	@demo_rating FLOAT,
	@demo_delivery FLOAT,
	@demo_net_cost_cpm MONEY,
	@lock_rate BIT,
	@minimum_cpm MONEY
AS
BEGIN
	INSERT INTO [dbo].[network_rate_card_details]
	(
		[network_rate_card_id],
		[tier],
		[network_id],
		[primary_audience_id],
		[hh_us_universe],
		[hh_coverage_universe],
		[hh_cpm],
		[hh_rating],
		[hh_delivery],
		[hh_net_cost_cpm],
		[demo_us_universe],
		[demo_coverage_universe],
		[demo_cpm],
		[demo_rating],
		[demo_delivery],
		[demo_net_cost_cpm],
		[lock_rate],
		[minimum_cpm]
	)
	VALUES
	(
		@network_rate_card_id,
		@tier,
		@network_id,
		@primary_audience_id,
		@hh_us_universe,
		@hh_coverage_universe,
		@hh_cpm,
		@hh_rating,
		@hh_delivery,
		@hh_net_cost_cpm,
		@demo_us_universe,
		@demo_coverage_universe,
		@demo_cpm,
		@demo_rating,
		@demo_delivery,
		@demo_net_cost_cpm,
		@lock_rate,
		@minimum_cpm
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
