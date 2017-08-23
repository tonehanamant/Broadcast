CREATE PROCEDURE usp_tam_post_analysis_reports_isci_networks_update
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@network_id		Int,
	@material_id		Int,
	@enabled		Bit,
	@subscribers		BigInt,
	@delivery		Float,
	@eq_delivery		Float,
	@units		Float,
	@dr_delivery		Float,
	@dr_eq_delivery		Float
)
AS
UPDATE tam_post_analysis_reports_isci_networks SET
	subscribers = @subscribers,
	delivery = @delivery,
	eq_delivery = @eq_delivery,
	units = @units,
	dr_delivery = @dr_delivery,
	dr_eq_delivery = @dr_eq_delivery
WHERE
	tam_post_proposal_id = @tam_post_proposal_id AND
	audience_id = @audience_id AND
	network_id = @network_id AND
	material_id = @material_id AND
	enabled = @enabled
