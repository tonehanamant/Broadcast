
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/9/2010
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[udf_GetMaterialAssociations]
(	
)
RETURNS TABLE 
AS
RETURN (
	with
	material_associations(
		material_id,
		associated_material_id,
		association_level
	) as (
		select
			materials.id material_id,
			materials.id associated_material_id,
			0 association_level
		from
			materials
			left join material_revisions on
				materials.id = material_revisions.original_material_id
		where
			material_revisions.revised_material_id is null
		union all
		select
			material_associations.material_id material_id,
			material_revisions.original_material_id alternate_material_id,
			material_associations.association_level + 1 association_level
		from
			material_associations
			join material_revisions on
				material_associations.associated_material_id = material_revisions.revised_material_id
	)
	select
		material_associations.material_id material_id,
		material_associations.associated_material_id associated_material_id,
		material_associations.association_level association_level
	from
		material_associations
);
