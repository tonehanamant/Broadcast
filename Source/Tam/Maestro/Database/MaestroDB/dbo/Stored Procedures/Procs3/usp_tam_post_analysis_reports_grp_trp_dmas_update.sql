CREATE PROCEDURE usp_tam_post_analysis_reports_grp_trp_dmas_update
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@dma_id		Int,
	@media_week_id		Int,
	@enabled		Bit,
	@subscribers		BigInt,
	@delivery		Float,
	@eq_delivery		Float,
	@units		Float,
	@dr_delivery		Float,
	@dr_eq_delivery		Float,
	@grp		Float,
	@eq_grp		Float,
	@dr_grp		Float,
	@dr_eq_grp		Float,
	@trp		Float,
	@eq_trp		Float,
	@dr_trp		Float,
	@dr_eq_trp		Float
)
AS
UPDATE tam_post_analysis_reports_grp_trp_dmas SET
	subscribers = @subscribers,
	delivery = @delivery,
	eq_delivery = @eq_delivery,
	units = @units,
	dr_delivery = @dr_delivery,
	dr_eq_delivery = @dr_eq_delivery,
	grp = @grp,
	eq_grp = @eq_grp,
	dr_grp = @dr_grp,
	dr_eq_grp = @dr_eq_grp,
	trp = @trp,
	eq_trp = @eq_trp,
	dr_trp = @dr_trp,
	dr_eq_trp = @dr_eq_trp
WHERE
	tam_post_proposal_id = @tam_post_proposal_id AND
	audience_id = @audience_id AND
	dma_id = @dma_id AND
	media_week_id = @media_week_id AND
	enabled = @enabled
