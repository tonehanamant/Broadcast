CREATE PROCEDURE usp_traffic_categories_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63)
)
AS
INSERT INTO traffic_categories
(
	name
)
VALUES
(
	@name
)

SELECT
	@id = SCOPE_IDENTITY()

