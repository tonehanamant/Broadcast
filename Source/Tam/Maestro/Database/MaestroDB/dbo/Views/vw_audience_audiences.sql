CREATE VIEW [dbo].[vw_audience_audiences] AS
	SELECT	
		a_c.id custom_audience_id,
		a_r.id rating_audience_id
	FROM
		audiences a_c (NOLOCK)
		JOIN audiences a_r (NOLOCK) ON
			a_r.range_end > a_c.range_start
			AND
			a_r.range_start < a_c.range_end
	WHERE
		(
			(a_c.sub_category_code = 'h' and a_r.sub_category_code in ( 'h' ))
			OR
			(a_c.sub_category_code = 'k' and a_r.sub_category_code in ( 'm', 'f' ))
			OR
			(a_c.sub_category_code = 't' and a_r.sub_category_code in ( 'm', 'f' ))
			OR
			(a_c.sub_category_code = 'a' and a_r.sub_category_code in ( 'm', 'f' ))
			OR
			(a_c.sub_category_code = 'p' and a_r.sub_category_code in ( 'm', 'f' ))
			OR
			(a_c.sub_category_code = 'w' and a_r.sub_category_code in ( 'f' ))
			OR
			(a_c.sub_category_code = 'f' and a_r.sub_category_code in ( 'f' ))
			OR
			(a_c.sub_category_code = 'm' and a_r.sub_category_code in ( 'm' ))
		)
		AND a_c.category_code = 0
		AND a_r.custom = 0
