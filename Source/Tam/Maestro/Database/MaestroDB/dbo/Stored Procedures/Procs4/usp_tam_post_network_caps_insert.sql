-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/10/2015 02:15:10 PM
-- Description:	Auto-generated method to insert a tam_post_network_caps record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_network_caps_insert]
	@tam_post_id INT,
	@network_id INT,
	@network_delivery_cap_percentage FLOAT,
	@bonus BIT
AS
BEGIN
	INSERT INTO [dbo].[tam_post_network_caps]
	(
		[tam_post_id],
		[network_id],
		[network_delivery_cap_percentage],
		[bonus]
	)
	VALUES
	(
		@tam_post_id,
		@network_id,
		@network_delivery_cap_percentage,
		@bonus
	)
END
