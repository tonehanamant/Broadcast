CREATE PROCEDURE [dbo].[usp_nielsen_county_universe_estimates_update]
(
	@document_id		Int,
	@line_number		Int,
	@effective_date		Date,
	@dma_name		VarChar(127),
	@dma_id		Int,
	@state		VarChar(7),
	@state_id		Int,
	@nmr_county_code		Int,
	@county_name		VarChar(127),
	@county_size		VarChar(7),
	@metro_indicator		VarChar(7),
	@tv_households		Int,
	@cable_households		Int,
	@cable_households_percentage		Float,
	@cable_and_or_ads_households		Int,
	@cable_and_or_ads_percentage		Float,
	@ads_households		Int,
	@ads_percentage		Float,
	@dbs_households		Int,
	@dbs_percentage		Float
)
AS
UPDATE nielsen_county_universe_estimates SET
	effective_date = @effective_date,
	dma_name = @dma_name,
	dma_id = @dma_id,
	state = @state,
	state_id = @state_id,
	nmr_county_code = @nmr_county_code,
	county_name = @county_name,
	county_size = @county_size,
	metro_indicator = @metro_indicator,
	tv_households = @tv_households,
	cable_households = @cable_households,
	cable_households_percentage = @cable_households_percentage,
	cable_and_or_ads_households = @cable_and_or_ads_households,
	cable_and_or_ads_percentage = @cable_and_or_ads_percentage,
	ads_households = @ads_households,
	ads_percentage = @ads_percentage,
	dbs_households = @dbs_households,
	dbs_percentage = @dbs_percentage
WHERE
	document_id = @document_id AND
	line_number = @line_number
