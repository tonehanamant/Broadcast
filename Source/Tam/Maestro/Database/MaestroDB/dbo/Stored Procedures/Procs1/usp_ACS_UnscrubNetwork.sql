CREATE PROCEDURE [dbo].[usp_ACS_UnscrubNetwork]
	@network_id INT,
	@media_month_id INT,
	@affidavit_net VARCHAR(63),
	@system_id INT
AS
BEGIN
	UPDATE
		a
	SET
		a.network_id=NULL,
		a.hash=NULL,
		a.status_id=2
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i  (NOLOCK) ON i.id=a.invoice_id
	WHERE
		a.media_month_id=@media_month_id
		AND a.network_id=@network_id
		AND a.affidavit_net=@affidavit_net
		AND i.system_id=@system_id

	DELETE FROM network_map_histories WHERE network_id=@network_id AND map_value=@affidavit_net AND map_set='Affidavits_' + CAST(@system_id AS VARCHAR(31))
	DELETE FROM network_maps WHERE network_id=@network_id AND map_value=@affidavit_net AND map_set='Affidavits_' + CAST(@system_id AS VARCHAR(31))
END
