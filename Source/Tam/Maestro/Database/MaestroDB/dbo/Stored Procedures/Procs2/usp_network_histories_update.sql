CREATE PROCEDURE usp_network_histories_update
(
	@network_id		Int,
	@start_date		DateTime,
	@code		VarChar(15),
	@name		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@end_date		DateTime,
	@language_id		TinyInt,
	@affiliated_network_id		Int,
	@network_type_id		TinyInt
)
AS
BEGIN
UPDATE dbo.network_histories SET
	code = @code,
	name = @name,
	active = @active,
	flag = @flag,
	end_date = @end_date,
	language_id = @language_id,
	affiliated_network_id = @affiliated_network_id,
	network_type_id = @network_type_id
WHERE
	network_id = @network_id AND
	start_date = @start_date
END
