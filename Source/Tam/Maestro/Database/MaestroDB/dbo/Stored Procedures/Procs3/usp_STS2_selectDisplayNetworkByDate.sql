-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- Changed on:	6/6/14
-- Changed By:	Brenton L Reeder
-- Changes:		Added language_id and affilaited_network_id to output
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayNetworkByDate]
	@network_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT	
		network_id,
		code,
		name,
		active,
		flag,
		start_date,
		end_date,		
		language_id, 
		affiliated_network_id,
		network_type_id
	FROM 
		uvw_network_universe (NOLOCK)
	WHERE 
		(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
		AND network_id=@network_id

	UNION

	SELECT	
		id,
		code,
		name,
		active,
		flag,
		effective_date,
		null,
		language_id, 
		affiliated_network_id,
		network_type_id
	FROM 
		networks (NOLOCK)
	WHERE 
		id NOT IN (
			SELECT network_id FROM uvw_network_universe (NOLOCK) WHERE
				(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
				AND network_id=@network_id
			)
		AND id=@network_id
END
