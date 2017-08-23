CREATE PROCEDURE usp_existing_mvpd_traffic_data_delete
(
	@traffic_id INT
	,@business_id INT = null
)
AS
BEGIN
	DECLARE @allocation_group_ids TABLE (traffic_spot_target_allocation_group_id INT)
	DECLARE @spot_target_ids TABLE (traffic_spot_target_id INT)

	IF (@business_id is null)
	BEGIN
		INSERT INTO @allocation_group_ids
		SELECT id
		FROM traffic_spot_target_allocation_group WITH(NOLOCK)
		WHERE traffic_id = @traffic_id
		
		DELETE FROM traffic_mvpd_ratings
		WHERE traffic_id = @traffic_id
		
		DELETE FROM traffic_mvpds
		WHERE traffic_id = @traffic_id
	END
	ELSE
	BEGIN
		INSERT INTO @allocation_group_ids
		SELECT id
		FROM traffic_spot_target_allocation_group WITH(NOLOCK)
		WHERE traffic_id = @traffic_id
		AND mvpd_id = @business_id
		
		DELETE FROM traffic_mvpd_ratings
		WHERE traffic_id = @traffic_id
		AND mvpd_id = @business_id
		
		DELETE FROM traffic_mvpds
		WHERE traffic_id = @traffic_id
		AND @business_id = @business_id
	END

	INSERT INTO @spot_target_ids
	SELECT id
	FROM traffic_spot_targets tst WITH(NOLOCK)
	INNER JOIN @allocation_group_ids ag ON tst.traffic_spot_target_allocation_group_id = ag.traffic_spot_target_allocation_group_id

	DELETE tstm 
	FROM traffic_spot_target_markets tstm WITH(ROWLOCK)
	INNER JOIN @allocation_group_ids ag ON tstm.traffic_spot_target_allocation_group_id = ag.traffic_spot_target_allocation_group_id

	DELETE tsta
	FROM traffic_spot_target_audiences tsta WITH(ROWLOCK)
	INNER JOIN @spot_target_ids st ON tsta.traffic_spot_target_id = st.traffic_spot_target_id

	DELETE tst
	FROM traffic_spot_targets tst WITH(ROWLOCK) 
	INNER JOIN @spot_target_ids st ON tst.id = st.traffic_spot_target_id
	WHERE traffic_id = @traffic_id

	DELETE tstag 
	FROM traffic_spot_target_allocation_group tstag WITH(ROWLOCK) 
	INNER JOIN @allocation_group_ids ag ON tstag.id = ag.traffic_spot_target_allocation_group_id
	WHERE traffic_id = @traffic_id
	
END