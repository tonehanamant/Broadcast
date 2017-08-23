CREATE PROCEDURE [dbo].[usp_MCS_CreateNotesForReel]
      @material_id INT
AS
BEGIN

declare @mytype varchar(15);
SELECT @mytype = type from materials with (NOLOCK) where id = @material_id;

IF @mytype = 'Married'
BEGIN
      SELECT
            CASE WHEN m2.code IS NULL THEN materials.code ELSE m2.code END
      FROM
            material_revisions WITH (NOLOCK)
            JOIN materials WITH (NOLOCK) ON materials.id=material_revisions.revised_material_id
            LEFT JOIN materials m2 WITH (NOLOCK) on materials.real_material_id = m2.id
      WHERE
            material_revisions.original_material_id=@material_id
      ORDER BY
            material_revisions.ordinal
END
ELSE
BEGIN
      SELECT M2.code from materials m1 WITH (NOLOCK) 
            join materials m2 WITH (NOLOCK) on m1.real_material_id = m2.id
      WHERE
            m1.id = @material_id;   

END
END
