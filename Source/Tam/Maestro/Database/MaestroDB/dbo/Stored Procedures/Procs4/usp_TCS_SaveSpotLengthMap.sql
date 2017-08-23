-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_SaveSpotLengthMap
	@id INT,
	@spot_length_id INT,
	@map_set VARCHAR(63),
	@map_value VARCHAR(63),
	@effective_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @id IS NOT NULL
	BEGIN
		DECLARE @previous_effective_date DATETIME;
		SELECT @previous_effective_date = slm.effective_date FROM dbo.spot_length_maps slm (NOLOCK) WHERE slm.id=@id;

		-- potentionally create history record
		IF @effective_date > @previous_effective_date 
		BEGIN
			INSERT INTO dbo.spot_length_map_histories (spot_length_map_id, spot_length_id, map_set, map_value, active, start_date, end_date)
				SELECT
					slm.id,
					slm.spot_length_id,
					slm.map_set,
					slm.map_value,
					slm.active,
					@previous_effective_date 'start_date',
					DATEADD(day, -1, @effective_date) 'end_date'
				FROM
					dbo.spot_length_maps slm (NOLOCK)
				WHERE 
					slm.id=@id;
		END

		-- update current record
		UPDATE dbo.spot_length_maps SET
			map_value = @map_value,
			effective_date = @effective_date
		WHERE
			id = @id;
	END
	ELSE
	BEGIN
		-- insert new record
		INSERT INTO dbo.spot_length_maps (spot_length_id, map_set, map_value, active, effective_date) VALUES (@spot_length_id, @map_set, @map_value, 1, @effective_date);
	END
END
