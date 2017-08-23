	-- =============================================
	-- Author:		CRUD Creator
	-- Create date: 03/10/2015 03:00:10 PM
	-- Description:	Auto-generated method to update a broadcast_affidavits record.
	-- =============================================
	CREATE PROCEDURE usp_broadcast_affidavits_update
		@media_month_id INT,
		@id BIGINT,
		@broadcast_affidavit_file_id INT,
		@spot_length_id INT,
		@station VARCHAR(63),
		@market VARCHAR(63),
		@network_affilates VARCHAR(63),
		@air_date DATETIME,
		@air_time INT,
		@isci VARCHAR(63),
		@program VARCHAR(255),
		@phone_number VARCHAR(25),
		@campaign VARCHAR(63),
		@advertiser VARCHAR(127),
		@product VARCHAR(127),
		@invoice_number VARCHAR(63),
		@market_rank INT
	AS
	BEGIN
		UPDATE
			[dbo].[broadcast_affidavits]
		SET
			[media_month_id]=@media_month_id,
			[broadcast_affidavit_file_id]=@broadcast_affidavit_file_id,
			[spot_length_id]=@spot_length_id,
			[station]=@station,
			[market]=@market,
			[network_affilates]=@network_affilates,
			[air_date]=@air_date,
			[air_time]=@air_time,
			[isci]=@isci,
			[program]=@program,
			[phone_number]=@phone_number,
			[campaign]=@campaign,
			[advertiser]=@advertiser,
			[product]=@product,
			[invoice_number]=@invoice_number,
			[market_rank]=@market_rank
		WHERE
			[id]=@id
	END
