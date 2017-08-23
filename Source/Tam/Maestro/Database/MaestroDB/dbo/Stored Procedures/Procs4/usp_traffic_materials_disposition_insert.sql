CREATE PROCEDURE usp_traffic_materials_disposition_insert
(
	@id		Int		OUTPUT,
	@disposition		VarChar(127),
	@active		Bit,
	@sort_order		Int
)
AS
INSERT INTO traffic_materials_disposition
(
	disposition,
	active,
	sort_order
)
VALUES
(
	@disposition,
	@active,
	@sort_order
)

SELECT
	@id = SCOPE_IDENTITY()

