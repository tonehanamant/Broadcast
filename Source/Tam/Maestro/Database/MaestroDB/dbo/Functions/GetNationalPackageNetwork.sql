-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetNationalPackageNetwork]
(
	@idNetwork AS INT,
	@date AS DATETIME
)
RETURNS INT
AS
BEGIN
	-- Declare the return variable here
	DECLARE
		@result as int,
		@codeNetwork as varchar(15);

	SELECT
		@codeNetwork = code
	FROM
		dbo.uvw_network_universe
	WHERE
		@idNetwork = network_id
		AND
		@date BETWEEN start_date AND ISNULL(end_date, dbo.GetEndOfTimeDate());

	SELECT
		@result = tmp.network_id
	FROM
		(
			SELECT
				network_id
			FROM
				network_maps
			WHERE	
				@date >= effective_date
				and
				map_set = 'traffic'
				and
				@codeNetwork = map_value
			UNION
			SELECT
				network_id
			FROM
				network_map_histories
			WHERE	
				@date between start_date and end_date
				and
				map_set = 'traffic'
				and
				@codeNetwork = map_value
		) tmp;
		

	-- Return the result of the function
	RETURN @result;
END
