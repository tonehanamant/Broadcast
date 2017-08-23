CREATE PROCEDURE usp_tam_post_network_detail_audiences_insert
(
	@tam_post_network_detail_id		Int,
	@audience_id		Int,
	@delivery		Float,
	@eq_delivery		Float,
	@dr_delivery		Float,
	@dr_eq_delivery		Float
)
AS
INSERT INTO tam_post_network_detail_audiences
(
	tam_post_network_detail_id,
	audience_id,
	delivery,
	eq_delivery,
	dr_delivery,
	dr_eq_delivery
)
VALUES
(
	@tam_post_network_detail_id,
	@audience_id,
	@delivery,
	@eq_delivery,
	@dr_delivery,
	@dr_eq_delivery
)

