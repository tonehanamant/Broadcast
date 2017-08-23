
-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/08/2015 01:46:42 PM
-- Description:	Auto-generated method to update a traffic record.
-- =============================================
CREATE PROCEDURE usp_traffic_update
	@id INT,
	@status_id INT,
	@release_id INT,
	@audience_id INT,
	@original_traffic_id INT,
	@traffic_category_id INT,
	@ratings_daypart_id INT,
	@revision INT,
	@name VARCHAR(63),
	@display_name VARCHAR(63),
	@description VARCHAR(127),
	@comment VARCHAR(255),
	@priority INT,
	@start_date DATETIME,
	@end_date DATETIME,
	@date_created DATETIME,
	@date_last_modified DATETIME,
	@base_ratings_media_month_id INT,
	@internal_note_id INT,
	@external_note_id INT,
	@adsrecovery_base BIT,
	@percent_discount FLOAT,
	@rate_card_type_id INT,
	@sort_order INT,
	@product_description_id INT,
	@base_universe_media_month_id INT,
	@base_index_media_month_id INT,
	@make_good BIT,
	@net_factors DECIMAL(19,8),
	@plan_type TINYINT
AS
BEGIN
	UPDATE
		[dbo].[traffic]
	SET
		[status_id]=@status_id,
		[release_id]=@release_id,
		[audience_id]=@audience_id,
		[original_traffic_id]=@original_traffic_id,
		[traffic_category_id]=@traffic_category_id,
		[ratings_daypart_id]=@ratings_daypart_id,
		[revision]=@revision,
		[name]=@name,
		[display_name]=@display_name,
		[description]=@description,
		[comment]=@comment,
		[priority]=@priority,
		[start_date]=@start_date,
		[end_date]=@end_date,
		[date_created]=@date_created,
		[date_last_modified]=@date_last_modified,
		[base_ratings_media_month_id]=@base_ratings_media_month_id,
		[internal_note_id]=@internal_note_id,
		[external_note_id]=@external_note_id,
		[adsrecovery_base]=@adsrecovery_base,
		[percent_discount]=@percent_discount,
		[rate_card_type_id]=@rate_card_type_id,
		[sort_order]=@sort_order,
		[product_description_id]=@product_description_id,
		[base_universe_media_month_id]=@base_universe_media_month_id,
		[base_index_media_month_id]=@base_index_media_month_id,
		[make_good]=@make_good,
		[net_factors]=@net_factors,
		[plan_type]=@plan_type
	WHERE
		[id]=@id
END
