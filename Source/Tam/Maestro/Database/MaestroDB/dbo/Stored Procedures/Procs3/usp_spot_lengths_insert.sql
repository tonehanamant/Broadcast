CREATE PROCEDURE usp_spot_lengths_insert
(
	@id		Int		OUTPUT,
	@length		Int,
	@delivery_multiplier		Float,
	@order_by		Int,
	@is_default		Bit
)
AS
INSERT INTO spot_lengths
(
	length,
	delivery_multiplier,
	order_by,
	is_default
)
VALUES
(
	@length,
	@delivery_multiplier,
	@order_by,
	@is_default
)

SELECT
	@id = SCOPE_IDENTITY()

