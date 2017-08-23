CREATE PROCEDURE usp_networks_update
(
	@id		Int,
	@code		VarChar(15),
	@name		VarChar(63),
	@active		Bit,
	@flag		TinyInt,
	@effective_date		DateTime,
	@language_id		TinyInt,
	@affiliated_network_id		Int,
	@network_type_id		TinyInt
)
AS
BEGIN
UPDATE dbo.networks SET
	code = @code,
	name = @name,
	active = @active,
	flag = @flag,
	effective_date = @effective_date,
	language_id = @language_id,
	affiliated_network_id = @affiliated_network_id,
	network_type_id = @network_type_id
WHERE
	id = @id

END
