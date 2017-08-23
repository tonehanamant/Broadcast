CREATE PROCEDURE [dbo].[usp_clusters_update]
(
	@id		Int,
	@name	VarChar(63),
	@code	varchar(32),
	@topography_id int,
	@cluster_type bit
)
AS
BEGIN
UPDATE clusters SET
	name = @name,
	code = @code,
	topography_id = @topography_id,
	cluster_type = @cluster_type
WHERE
	id = @id
END

