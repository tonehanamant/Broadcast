CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_iscis_insert]
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@network_id		Int,
	@material_id		Int,
	@isci_material_id		Int,
	@enabled		Bit,
	@subscribers		BigInt,
	@delivery		Float,
	@eq_delivery		Float,
	@units	Float,
	@dr_delivery		Float,
	@dr_eq_delivery		Float,
	@total_spots		Int
)
AS
INSERT INTO tam_post_analysis_reports_iscis
(
	tam_post_proposal_id,
	audience_id,
	network_id,
	material_id,
	isci_material_id,
	enabled,
	subscribers,
	delivery,
	eq_delivery,
	units,
	dr_delivery,
	dr_eq_delivery,
	total_spots
)
VALUES
(
	@tam_post_proposal_id,
	@audience_id,
	@network_id,
	@material_id,
	@isci_material_id,
	@enabled,
	@subscribers,
	@delivery,
	@eq_delivery,
	@units,
	@dr_delivery,
	@dr_eq_delivery,
	@total_spots
)
