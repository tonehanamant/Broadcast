-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/10/2015 02:15:10 PM
-- Description:	Auto-generated method to update a tam_post_network_caps record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_network_caps_update]
	@tam_post_id INT,
	@network_id INT,
	@network_delivery_cap_percentage FLOAT,
	@bonus BIT
AS
BEGIN
	UPDATE
		[dbo].[tam_post_network_caps]
	SET
		[network_delivery_cap_percentage]=@network_delivery_cap_percentage,
		[bonus]=@bonus
	WHERE
		[tam_post_id]=@tam_post_id
		AND [network_id]=@network_id
END
