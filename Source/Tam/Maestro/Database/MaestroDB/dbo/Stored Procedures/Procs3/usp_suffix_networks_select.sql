	CREATE PROCEDURE usp_suffix_networks_select
	(
		@suffix varchar(1)
	)
	AS
	BEGIN
		
		SELECT * 
		FROM networks (nolock)
		WHERE active = 1
		AND code LIKE '%-' + @suffix
		ORDER BY code
	END
