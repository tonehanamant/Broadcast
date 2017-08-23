-- usp_MCS_DoesMaterialExist 'TAM00001', null
CREATE PROCEDURE [dbo].[usp_MCS_DoesMaterialExist]
	@copy VARCHAR(31),
	@excluded_material_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT COUNT(*) FROM materials WHERE code=@copy AND (@excluded_material_id IS NULL OR id<>@excluded_material_id)
END
