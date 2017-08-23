CREATE PROCEDURE usp_cmw_traffic_product_descriptions_insert
(
	@id		Int		OUTPUT,
	@product_description		VarChar(127)
)
AS
INSERT INTO cmw_traffic_product_descriptions
(
	product_description
)
VALUES
(
	@product_description
)

SELECT
	@id = SCOPE_IDENTITY()

