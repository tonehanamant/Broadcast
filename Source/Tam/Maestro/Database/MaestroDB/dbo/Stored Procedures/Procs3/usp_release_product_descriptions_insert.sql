CREATE PROCEDURE usp_release_product_descriptions_insert
(
	@id		Int		OUTPUT,
	@product_description		VarChar(63)
)
AS
INSERT INTO release_product_descriptions
(
	product_description
)
VALUES
(
	@product_description
)

SELECT
	@id = SCOPE_IDENTITY()

