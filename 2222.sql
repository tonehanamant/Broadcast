select * from plan_versions

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND (COLUMN_NAME= 'fluidity_percentage' OR COLUMN_NAME='category' OR COLUMN_NAME='fluidity_child_category'))
BEGIN
	ALTER TABLE plan_versions
	ADD fluidity_percentage float NULL,
	category int NULL,
	fluidity_child_category int NULL
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_pricing_parameters' AND (COLUMN_NAME= 'fluidity_percentage' OR COLUMN_NAME='category' OR COLUMN_NAME='fluidity_child_category'))
BEGIN
	ALTER TABLE plan_version_pricing_parameters
	ADD fluidity_percentage float NULL,
	category int NULL,
	fluidity_child_category int NULL
END

select * from plan_version_pricing_parameters
IF EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'category'
        AND OBJECT_ID = OBJECT_ID('plan_version_pricing_parameters'))
BEGIN
	ALTER TABLE plan_version_pricing_parameters
		DROP COLUMN category
	ALTER TABLE plan_version_pricing_parameters
		DROP COLUMN fluidity_child_category
END

IF EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'category'
        AND OBJECT_ID = OBJECT_ID('plan_versions'))
BEGIN
	EXEC sp_rename 'plan_versions.category', 'fluidity_category', 'COLUMN'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_buying_parameters' AND (COLUMN_NAME= 'fluidity_percentage'))
BEGIN
	ALTER TABLE plan_version_buying_parameters
	ADD fluidity_percentage float NULL
END