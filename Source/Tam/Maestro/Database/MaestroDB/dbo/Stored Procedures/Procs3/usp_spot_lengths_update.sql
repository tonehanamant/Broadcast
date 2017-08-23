CREATE PROCEDURE usp_spot_lengths_update
(
	@id		Int,
	@length		Int,
	@delivery_multiplier		Float,
	@order_by		Int,
	@is_default		Bit
)
AS
UPDATE spot_lengths SET
	length = @length,
	delivery_multiplier = @delivery_multiplier,
	order_by = @order_by,
	is_default = @is_default
WHERE
	id = @id

