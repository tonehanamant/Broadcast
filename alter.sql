IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND (COLUMN_NAME= 'market_code' OR COLUMN_NAME='market_rank'))
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
	ADD market_code int NULL,
	market_rank int NULL
END

select * from spot_exceptions_out_of_specs