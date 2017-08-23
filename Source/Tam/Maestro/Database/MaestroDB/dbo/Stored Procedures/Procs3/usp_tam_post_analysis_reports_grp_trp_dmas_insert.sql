CREATE PROCEDURE usp_tam_post_analysis_reports_grp_trp_dmas_insert
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
INSERT INTO tam_post_analysis_reports_grp_trp_dmas
(
	tam_post_proposal_id,
	audience_id,
	dma_id,
	media_week_id,
	enabled,
	subscribers,
	delivery,
	eq_delivery,
	units,
	dr_delivery,
	dr_eq_delivery,
	grp,
	eq_grp,
	dr_grp,
	dr_eq_grp,
	trp,
	eq_trp,
	dr_trp,
	dr_eq_trp
)
VALUES
(
	@tam_post_proposal_id,
	@audience_id,
	@dma_id,
	@media_week_id,
	@enabled,
	@subscribers,
	@delivery,
	@eq_delivery,
	@units,
	@dr_delivery,
	@dr_eq_delivery,
	@grp,
	@eq_grp,
	@dr_grp,
	@dr_eq_grp,
	@trp,
	@eq_trp,
	@dr_trp,
	@dr_eq_trp
)

