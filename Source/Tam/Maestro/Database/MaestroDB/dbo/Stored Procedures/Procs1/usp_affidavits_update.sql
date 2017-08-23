-- =============================================
-- Author:		CRUD Creator
-- Create date: 01/23/2017 10:04:29 AM
-- Description:	Auto-generated method to update a affidavits record.
-- =============================================
CREATE PROCEDURE [usp_affidavits_update]
	@id BIGINT,
	@media_month_id INT,
	@status_id TINYINT,
	@invoice_id INT,
	@traffic_id INT,
	@material_id INT,
	@zone_id INT,
	@network_id INT,
	@spot_length_id TINYINT,
	@air_date DATETIME,
	@air_time INT,
	@rate INT,
	@affidavit_file_line INT,
	@affidavit_air_date VARCHAR(31),
	@affidavit_air_time VARCHAR(31),
	@affidavit_length VARCHAR(15),
	@affidavit_copy VARCHAR(63),
	@affidavit_net VARCHAR(15),
	@affidavit_syscode VARCHAR(15),
	@affidavit_rate VARCHAR(15),
	@hash CHAR(59),
	@subscribers INT,
	@program_name VARCHAR(63),
	@adjusted_air_date DATE,
	@adjusted_air_time INT,
	@gmt_air_datetime DATETIME,
	@gracenote_schedule_id INT
AS
BEGIN
	UPDATE
		[dbo].[affidavits]
	SET
		[media_month_id]=@media_month_id,
		[status_id]=@status_id,
		[invoice_id]=@invoice_id,
		[traffic_id]=@traffic_id,
		[material_id]=@material_id,
		[zone_id]=@zone_id,
		[network_id]=@network_id,
		[spot_length_id]=@spot_length_id,
		[air_date]=@air_date,
		[air_time]=@air_time,
		[rate]=@rate,
		[affidavit_file_line]=@affidavit_file_line,
		[affidavit_air_date]=@affidavit_air_date,
		[affidavit_air_time]=@affidavit_air_time,
		[affidavit_length]=@affidavit_length,
		[affidavit_copy]=@affidavit_copy,
		[affidavit_net]=@affidavit_net,
		[affidavit_syscode]=@affidavit_syscode,
		[affidavit_rate]=@affidavit_rate,
		[hash]=@hash,
		[subscribers]=@subscribers,
		[program_name]=@program_name,
		[adjusted_air_date]=@adjusted_air_date,
		[adjusted_air_time]=@adjusted_air_time,
		[gmt_air_datetime]=@gmt_air_datetime,
		[gracenote_schedule_id]=@gracenote_schedule_id
	WHERE
		[id]=@id
END
