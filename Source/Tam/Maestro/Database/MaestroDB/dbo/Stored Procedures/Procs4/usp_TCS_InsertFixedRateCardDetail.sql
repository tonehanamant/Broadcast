
CREATE Procedure [usp_TCS_InsertFixedRateCardDetail]
(
	@traffic_id int,	
	@topography_id int,
	@traffic_rate_card_id int
)
AS
	INSERT INTO traffic_topography_rate_card_map(traffic_id, topography_id, traffic_rate_card_id)
	VALUES (@traffic_id, @topography_id, @traffic_rate_card_id);

