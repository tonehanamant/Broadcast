
-----------------END CRMWTRF-116 - Enter Traffic Notes ------------------------------------------------------------------

-----------------START CRMWTRF-961 - New releases not showing up under tree view release composer

CREATE PROCEDURE [dbo].[usp_REL_GetReleasesByMediaMonth]
(
	@media_month_id int
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED  -- same as NOLOCK
		
	SELECT 
		rel.id, 
		rel.category_id,
		rel.status_id,
		rel.name,
		rel.description,
		rel.notes,
		rel.release_date,
		rel.confirm_by_date
	FROM 
		releases rel
	WHERE 
		rel.name like '%' + (SELECT media_month FROM media_months where id = @media_month_id) + '%'
	UNION
	SELECT 
		rel2.id, 
		rel2.category_id,
		rel2.status_id,
		rel2.name,
		rel2.description,
		rel2.notes,
		rel2.release_date,
		rel2.confirm_by_date
	FROM releases rel2
		 JOIN media_months mm on mm.id = @media_month_id
		 JOIN (SELECT r.id, MIN(t.start_date) AS min_date
				FROM releases r
				INNER JOIN traffic t ON r.id = t.release_id
				GROUP BY r.id) t ON t.id = rel2.id
	WHERE t.min_date >= mm.start_date
	AND   t.min_date <= mm.end_date
	GROUP BY
		rel2.id, 
		rel2.category_id,
		rel2.status_id,
		rel2.name,
		rel2.description,
		rel2.notes,
		rel2.release_date,
		rel2.confirm_by_date
	ORDER BY 
		rel.name
END
