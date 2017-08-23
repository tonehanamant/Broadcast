-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- Changed:		6/10/2014
-- Changed by:	Brenton L Reeder
-- Changes:		Added LangId, AffNetworkId, and NetworkTypeId
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayNetworksByDate]
	@active bit,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	(
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
			(@active IS NULL OR active=@active)
			AND (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
		
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
			(@active IS NULL OR active=@active)
			AND networks.id NOT IN (
				SELECT network_id FROM uvw_network_universe (NOLOCK) WHERE
					(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
				)
	) ORDER BY name
END
