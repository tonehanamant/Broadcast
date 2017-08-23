	-- =============================================
	-- Author:		CRUD Creator
	-- Create date: 03/10/2015 03:00:09 PM
	-- Description:	Auto-generated method to insert a broadcast_affidavits record.
	-- =============================================
	CREATE PROCEDURE usp_broadcast_affidavits_insert
		@media_month_id INT,
		@id BIGINT OUTPUT,
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
		INSERT INTO [dbo].[broadcast_affidavits]
		(
			[media_month_id],
			[broadcast_affidavit_file_id],
			[spot_length_id],
			[station],
			[market],
			[network_affilates],
			[air_date],
			[air_time],
			[isci],
			[program],
			[phone_number],
			[campaign],
			[advertiser],
			[product],
			[invoice_number],
			[market_rank]
		)
		VALUES
		(
			@media_month_id,
			@broadcast_affidavit_file_id,
			@spot_length_id,
			@station,
			@market,
			@network_affilates,
			@air_date,
			@air_time,
			@isci,
			@program,
			@phone_number,
			@campaign,
			@advertiser,
			@product,
			@invoice_number,
			@market_rank
		)

		SELECT
			@id = SCOPE_IDENTITY()
	END
