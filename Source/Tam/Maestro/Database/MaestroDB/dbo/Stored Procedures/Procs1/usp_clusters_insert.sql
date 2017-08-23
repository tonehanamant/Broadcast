CREATE PROCEDURE [dbo].[usp_clusters_insert]
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@code varchar(32),
	@topography_id int,
	@cluster_type bit
)
AS
BEGIN
INSERT INTO clusters
(
	name,
	code,
	topography_id,
	cluster_type
)
VALUES
(
	@name,
	@code,
	@topography_id,
	@cluster_type
)

SELECT
	@id = SCOPE_IDENTITY()
END

