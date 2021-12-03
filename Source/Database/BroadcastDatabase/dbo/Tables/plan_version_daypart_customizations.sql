CREATE TABLE plan_version_daypart_customizations
(
	id INT IDENTITY(1,1) ,
	plan_version_daypart_id INT NOT NULL,
	custom_daypart_organization_id INT NOT NULL, 
	custom_daypart_name NVARCHAR(100) NOT NULL,
	CONSTRAINT [FK_plan_version_daypart_customizations_plan_version_daypart] FOREIGN KEY([plan_version_daypart_id])REFERENCES [dbo].[plan_version_dayparts] ([id]) ON DELETE CASCADE,
	CONSTRAINT [FK_plan_version_daypart_customizations_custom_daypart_organizations] FOREIGN KEY([custom_daypart_organization_id])REFERENCES [dbo].[custom_daypart_organizations] ([id]),
	CONSTRAINT UQ_plan_version_daypart_customizations_plan_version_daypart_id UNIQUE (plan_version_daypart_id), 
    PRIMARY KEY ([plan_version_daypart_id])
)