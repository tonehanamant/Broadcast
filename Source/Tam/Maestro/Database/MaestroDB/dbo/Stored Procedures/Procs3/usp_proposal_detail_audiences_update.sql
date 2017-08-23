CREATE PROCEDURE usp_proposal_detail_audiences_update
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
UPDATE proposal_detail_audiences SET
	rating = @rating,
	vpvh = @vpvh,
	coverage_delivery = @coverage_delivery,
	us_delivery = @us_delivery,
	us_universe = @us_universe
WHERE
	proposal_detail_id = @proposal_detail_id AND
	audience_id = @audience_id
