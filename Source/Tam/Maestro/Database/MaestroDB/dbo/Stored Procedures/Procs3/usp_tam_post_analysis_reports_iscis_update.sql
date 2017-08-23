CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_iscis_update]
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
	@units		Float,
	@dr_delivery		Float,
	@dr_eq_delivery		Float,
	@total_spots		Int
)
AS
UPDATE tam_post_analysis_reports_iscis SET
	subscribers = @subscribers,
	delivery = @delivery,
	eq_delivery = @eq_delivery,
	units = @units,
	dr_delivery = @dr_delivery,
	dr_eq_delivery = @dr_eq_delivery,
	total_spots = @total_spots
WHERE
	tam_post_proposal_id = @tam_post_proposal_id AND
	audience_id = @audience_id AND
	network_id = @network_id AND
	material_id = @material_id AND
	isci_material_id = @isci_material_id AND
	enabled = @enabled
