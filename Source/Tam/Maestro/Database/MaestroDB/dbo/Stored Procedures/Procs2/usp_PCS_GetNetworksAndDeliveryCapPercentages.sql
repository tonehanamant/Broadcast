CREATE PROCEDURE [dbo].[usp_PCS_GetNetworksAndDeliveryCapPercentages]
	@proposal_ids varchar(max),
	@tam_post_id int
AS
BEGIN 
	SET NOCOUNT ON;

     SELECT
		n.id,
		n.code,
		n.name,
		@tam_post_id,
		tpnc.network_delivery_cap_percentage,
		tpnc.bonus
	FROM 
		dbo.networks n (NOLOCK)
		LEFT JOIN dbo.tam_post_network_caps tpnc (NOLOCK) ON tpnc.network_id = n.id 
			AND tpnc.tam_post_id = @tam_post_id
	WHERE
		n.id IN (
			SELECT DISTINCT 
				pd.network_id 
			FROM 
				proposal_details pd (NOLOCK)
			WHERE
				pd.proposal_id IN (
					SELECT id FROM dbo.SplitIntegers(@proposal_ids)
				)
		)
END
