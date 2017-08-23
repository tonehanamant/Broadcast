﻿
-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/06/2015 12:15:41 AM
-- Description:	Auto-generated method to update a traffic_details record.
-- =============================================
CREATE PROCEDURE usp_traffic_details_update
	@id INT,
	@traffic_id INT,
	@network_id INT,
	@daypart_id INT,
	@spot_length_id INT,
	@comment VARCHAR(127),
	@internal_note_id INT,
	@external_note_id INT,
	@traffic_amount MONEY,
	@release_amount MONEY,
	@CPM1 MONEY,
	@CPM2 MONEY,
	@traffic_amount1 MONEY,
	@traffic_amount2 MONEY,
	@release_amount1 MONEY,
	@release_amount2 MONEY,
	@proposal_detail_id INT
AS
BEGIN
	UPDATE
		[dbo].[traffic_details]
	SET
		[traffic_id]=@traffic_id,
		[network_id]=@network_id,
		[daypart_id]=@daypart_id,
		[spot_length_id]=@spot_length_id,
		[comment]=@comment,
		[internal_note_id]=@internal_note_id,
		[external_note_id]=@external_note_id,
		[traffic_amount]=@traffic_amount,
		[release_amount]=@release_amount,
		[CPM1]=@CPM1,
		[CPM2]=@CPM2,
		[traffic_amount1]=@traffic_amount1,
		[traffic_amount2]=@traffic_amount2,
		[release_amount1]=@release_amount1,
		[release_amount2]=@release_amount2,
		[proposal_detail_id]=@proposal_detail_id
	WHERE
		[id]=@id
END
