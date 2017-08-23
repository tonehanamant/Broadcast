-- =============================================
-- Author:		CRUD Creator
-- Create date: 09/21/2015 10:29:51 AM
-- Description:	Auto-generated method to update a tam_post_proposals record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_proposals_update]
	@id INT,
	@tam_post_id INT,
	@posting_plan_proposal_id INT,
	@post_source_code TINYINT,
	@total_spots_in_spec INT,
	@total_spots_out_of_spec INT,
	@posted_by_employee_id INT,
	@post_duration FLOAT,
	@post_started DATETIME,
	@post_completed DATETIME,
	@aggregation_status_code TINYINT,
	@aggregation_duration FLOAT,
	@aggregation_started DATETIME,
	@aggregation_completed DATETIME,
	@number_of_zones_delivering INT,
	@date_exported_to_msa DATETIME,
	@msa_status_code TINYINT
AS
BEGIN
	UPDATE
		[dbo].[tam_post_proposals]
	SET
		[tam_post_id]=@tam_post_id,
		[posting_plan_proposal_id]=@posting_plan_proposal_id,
		[post_source_code]=@post_source_code,
		[total_spots_in_spec]=@total_spots_in_spec,
		[total_spots_out_of_spec]=@total_spots_out_of_spec,
		[posted_by_employee_id]=@posted_by_employee_id,
		[post_duration]=@post_duration,
		[post_started]=@post_started,
		[post_completed]=@post_completed,
		[aggregation_status_code]=@aggregation_status_code,
		[aggregation_duration]=@aggregation_duration,
		[aggregation_started]=@aggregation_started,
		[aggregation_completed]=@aggregation_completed,
		[number_of_zones_delivering]=@number_of_zones_delivering,
		[date_exported_to_msa]=@date_exported_to_msa,
		[msa_status_code]=@msa_status_code
	WHERE
		[id]=@id
END
