CREATE PROCEDURE [dbo].[usp_STS_upsert_topography_maps]
(
	@id				int		OUTPUT,
	@idTopography	int,
	@mapSet			VarChar(20),
	@mapValue		VarChar(3)
)
AS
	SET NOCOUNT ON;

	SET @id = NULL;

	--Adding coverage_universe mapset entry to topography_maps if necessary
	SELECT	
		@id = id 
	FROM 
		topography_maps 
	WHERE 
		topography_id = @idTopography 
		AND 
		map_set = @mapSet;

--	PRINT 'Upserting record #' + cast(@id as varchar) + ', topography #' + cast(@idTopography as varchar) + ', map set ' + @mapSet + ', map value ' + @mapValue;
	IF @id IS NOT NULL
		BEGIN
		UPDATE
			topography_maps
		SET
			map_value = @mapValue
		WHERE
			topography_id = @idTopography
			AND
			map_set = @mapSet
			AND
			map_value <> @mapValue;
--		PRINT 'Updated topography_maps.';
		END
	ELSE
		BEGIN
		INSERT INTO
			topography_maps(
				topography_id, 
				map_set, 
				map_value
			)
			VALUES(
				@idTopography, 
				@mapSet, 
				@mapValue
			);
		SET @id = SCOPE_IDENTITY();
--		PRINT 'Inserted into topography_maps.';
		END
