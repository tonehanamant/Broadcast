	CREATE PROCEDURE usp_traditional_networks_select
	AS
	BEGIN

		SELECT * 
		FROM networks (nolock)
		WHERE active = 1
		AND (code NOT LIKE '%-C' 
				AND code NOT LIKE '%-H')
		ORDER BY code
	END
