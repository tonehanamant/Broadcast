
CREATE PROCEDURE [dbo].[usp_system_sales_model_maps_insert]
(
	@id		int		OUTPUT,
	@system_id		Int,
	@sales_model_id		Int,
	@map_set		VarChar(63),
	@map_value		VARCHAR(MAX),
	@effective_date		DateTime
)
AS
INSERT INTO system_sales_model_maps
(
	system_id,
	sales_model_id,
	map_set,
	map_value,
	effective_date
)
VALUES
(
	@system_id,
	@sales_model_id,
	@map_set,
	@map_value,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()


