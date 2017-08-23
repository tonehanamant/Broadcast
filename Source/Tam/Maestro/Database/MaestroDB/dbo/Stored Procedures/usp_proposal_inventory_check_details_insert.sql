﻿-- =============================================
-- Author:		CRUD Creator
-- Create date: 02/26/2016 11:22:45 AM
-- Description:	Auto-generated method to insert a proposal_inventory_check_details record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_inventory_check_details_insert]
	@proposal_id INT,
	@proposal_detail_id INT,
	@date_created DATETIME,
	@proposal_status_id INT,
	@base_media_month_id INT,
	@spot_length_id TINYINT,
	@network_id INT,
	@daypart_id INT,
	@rating_daypart_id INT,
	@proposal_rate MONEY,
	@coverage_universe INT,
	@hh_eq_cpm MONEY,
	@health_score FLOAT,
	@total_contracted_subscribers BIGINT,
	@total_contracted_units INT,
	@total_allocated_subscribers BIGINT,
	@total_allocated_units FLOAT,
	@total_forecasted_subscribers BIGINT,
	@total_forecasted_units FLOAT,
	@percentage_of_forecasted_inventory_exposed FLOAT,
	@percentage_of_forecasted_inventory_remaining FLOAT
AS
BEGIN
	INSERT INTO [dbo].[proposal_inventory_check_details]
	(
		[proposal_id],
		[proposal_detail_id],
		[date_created],
		[proposal_status_id],
		[base_media_month_id],
		[spot_length_id],
		[network_id],
		[daypart_id],
		[rating_daypart_id],
		[proposal_rate],
		[coverage_universe],
		[hh_eq_cpm],
		[health_score],
		[total_contracted_subscribers],
		[total_contracted_units],
		[total_allocated_subscribers],
		[total_allocated_units],
		[total_forecasted_subscribers],
		[total_forecasted_units],
		[percentage_of_forecasted_inventory_exposed],
		[percentage_of_forecasted_inventory_remaining]
	)
	VALUES
	(
		@proposal_id,
		@proposal_detail_id,
		@date_created,
		@proposal_status_id,
		@base_media_month_id,
		@spot_length_id,
		@network_id,
		@daypart_id,
		@rating_daypart_id,
		@proposal_rate,
		@coverage_universe,
		@hh_eq_cpm,
		@health_score,
		@total_contracted_subscribers,
		@total_contracted_units,
		@total_allocated_subscribers,
		@total_allocated_units,
		@total_forecasted_subscribers,
		@total_forecasted_units,
		@percentage_of_forecasted_inventory_exposed,
		@percentage_of_forecasted_inventory_remaining
	)
END