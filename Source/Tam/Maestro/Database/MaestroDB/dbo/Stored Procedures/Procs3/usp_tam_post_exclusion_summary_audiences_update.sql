CREATE PROCEDURE usp_tam_post_exclusion_summary_audiences_update
(
	@tam_post_exclusion_summary_id		Int,
	@audience_id		Int,
	@delivery		Float,
	@eq_delivery		Float,
	@dr_delivery		Float,
	@dr_eq_delivery		Float
)
AS
UPDATE tam_post_exclusion_summary_audiences SET
	delivery = @delivery,
	eq_delivery = @eq_delivery,
	dr_delivery = @dr_delivery,
	dr_eq_delivery = @dr_eq_delivery
WHERE
	tam_post_exclusion_summary_id = @tam_post_exclusion_summary_id AND
	audience_id = @audience_id
