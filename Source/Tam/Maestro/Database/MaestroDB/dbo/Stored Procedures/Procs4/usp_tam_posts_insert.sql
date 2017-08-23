﻿
-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/01/2015 12:40:29 PM
-- Description:	Auto-generated method to insert a tam_posts record.
-- =============================================
CREATE PROCEDURE usp_tam_posts_insert
	@id INT OUTPUT,
	@rating_source_id TINYINT,
	@rate_card_type_id INT,
	@post_type_code TINYINT,
	@title VARCHAR(127),
	@is_deleted BIT,
	@is_equivalized BIT,
	@network_delivery_cap_percentage FLOAT,
	@post_setup_advertiser VARCHAR(511),
	@post_setup_agency VARCHAR(511),
	@post_setup_daypart VARCHAR(511),
	@post_setup_product VARCHAR(511),
	@override_advertiser BIT,
	@override_agency BIT,
	@override_daypart BIT,
	@override_product BIT,
	@multiple_product_post BIT,
	@strict_start_time BIT,
	@strict_end_time BIT,
	@created_by_employee_id INT,
	@modified_by_employee_id INT,
	@deleted_by_employee_id INT,
	@locked BIT,
	@locked_by_employee_id INT,
	@number_of_zones_delivering INT,
	@date_created DATETIME,
	@date_last_modified DATETIME,
	@date_deleted DATETIME,
	@report_weekly_pacing BIT,
	@exclude_from_year_to_date_report BIT,
	@full_fast_tracks BIT,
	@is_msa BIT,
	@campaign_id INT,
	@produce_monthy_posts BIT,
	@produce_quarterly_posts BIT,
	@produce_full_posts BIT
AS
BEGIN
	INSERT INTO [dbo].[tam_posts]
	(
		[rating_source_id],
		[rate_card_type_id],
		[post_type_code],
		[title],
		[is_deleted],
		[is_equivalized],
		[network_delivery_cap_percentage],
		[post_setup_advertiser],
		[post_setup_agency],
		[post_setup_daypart],
		[post_setup_product],
		[override_advertiser],
		[override_agency],
		[override_daypart],
		[override_product],
		[multiple_product_post],
		[strict_start_time],
		[strict_end_time],
		[created_by_employee_id],
		[modified_by_employee_id],
		[deleted_by_employee_id],
		[locked],
		[locked_by_employee_id],
		[number_of_zones_delivering],
		[date_created],
		[date_last_modified],
		[date_deleted],
		[report_weekly_pacing],
		[exclude_from_year_to_date_report],
		[full_fast_tracks],
		[is_msa],
		[campaign_id],
		[produce_monthy_posts],
		[produce_quarterly_posts],
		[produce_full_posts]
	)
	VALUES
	(
		@rating_source_id,
		@rate_card_type_id,
		@post_type_code,
		@title,
		@is_deleted,
		@is_equivalized,
		@network_delivery_cap_percentage,
		@post_setup_advertiser,
		@post_setup_agency,
		@post_setup_daypart,
		@post_setup_product,
		@override_advertiser,
		@override_agency,
		@override_daypart,
		@override_product,
		@multiple_product_post,
		@strict_start_time,
		@strict_end_time,
		@created_by_employee_id,
		@modified_by_employee_id,
		@deleted_by_employee_id,
		@locked,
		@locked_by_employee_id,
		@number_of_zones_delivering,
		@date_created,
		@date_last_modified,
		@date_deleted,
		@report_weekly_pacing,
		@exclude_from_year_to_date_report,
		@full_fast_tracks,
		@is_msa,
		@campaign_id,
		@produce_monthy_posts,
		@produce_quarterly_posts,
		@produce_full_posts
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
