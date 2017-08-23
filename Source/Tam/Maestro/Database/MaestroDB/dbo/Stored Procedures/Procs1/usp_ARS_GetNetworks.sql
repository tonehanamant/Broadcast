-- =============================================
-- Author:        <Author,,Name>
-- Create date: <Create Date,,>
-- Description:   <Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetNetworks]
AS
BEGIN
		SELECT
			id,
			code,
			name,
			active,
			flag,
			effective_date,
			language_id,
			affiliated_network_id,
			network_type_id
		FROM
			networks
		ORDER BY
			code
END
