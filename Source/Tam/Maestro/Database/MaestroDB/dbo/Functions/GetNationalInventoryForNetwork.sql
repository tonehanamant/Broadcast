-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetNationalInventoryForNetwork]
(
	-- Add the parameters for the function here
	@network_id INT
)
RETURNS INT
AS
BEGIN
	-- Declare the return variable here
	DECLARE @national_units AS INT
	SET @national_units = 
	(
		SELECT SUM(national_units) FROM
		(
			SELECT
				(
					d.total_hours
					*
					-- total minutes per hour
					(SELECT SUM(length) / 60.0 FROM network_breaks (NOLOCK) WHERE nielsen_network_id=nnrd.nielsen_network_id)
				) 'national_units'
			FROM 
				nielsen_network_rating_dayparts nnrd (NOLOCK)
				JOIN vw_ccc_daypart d (NOLOCK) ON d.id=nnrd.daypart_id
			WHERE
				nnrd.nielsen_network_id IN (SELECT TOP 1 id FROM nielsen_networks (NOLOCK) WHERE nielsen_id = (SELECT TOP 1 CAST(map_value AS INT) FROM network_maps (NOLOCK) WHERE map_set='Nielsen' AND network_id=@network_id))
		) tmp
	)

	-- Return the result of the function
	RETURN @national_units
END
