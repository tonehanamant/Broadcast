create table test(id int ,username varchar)

select * from test

IF EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'product_name'
        AND OBJECT_ID = OBJECT_ID('reel_isci_products'))
BEGIN
	DROP TABLE reel_isci_products
END

select * from reel_isci_products