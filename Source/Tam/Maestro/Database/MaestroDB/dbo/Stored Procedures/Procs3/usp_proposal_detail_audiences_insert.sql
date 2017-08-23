CREATE PROCEDURE usp_proposal_detail_audiences_insert
(
	@proposal_detail_id		Int,
	@audience_id		Int,
	@rating		Float,
	@vpvh		Float,
	@coverage_delivery		Float,
	@us_delivery		Float,
	@us_universe		Float
)
AS
INSERT INTO proposal_detail_audiences
(
	proposal_detail_id,
	audience_id,
	rating,
	vpvh,
	coverage_delivery,
	us_delivery,
	us_universe
)
VALUES
(
	@proposal_detail_id,
	@audience_id,
	@rating,
	@vpvh,
	@coverage_delivery,
	@us_delivery,
	@us_universe
)

