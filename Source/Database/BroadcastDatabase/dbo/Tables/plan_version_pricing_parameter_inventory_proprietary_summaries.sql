CREATE TABLE [dbo].[plan_version_pricing_parameter_inventory_proprietary_summaries] (
    [id]                                INT        IDENTITY (1, 1) NOT NULL,
    [plan_version_pricing_parameter_id] INT        NOT NULL,
    [inventory_proprietary_summary_id]  INT        NOT NULL,
    [unit_number]                       FLOAT (53) NOT NULL,
    CONSTRAINT [PK_plan_version_pricing_parameter_inventory_proprietary_summaries] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_pricing_parameter_inventory_proprietary_summaries_inventory_proprietary_summary] FOREIGN KEY ([inventory_proprietary_summary_id]) REFERENCES [dbo].[inventory_proprietary_summary] ([id]),
    CONSTRAINT [FK_plan_version_pricing_parameter_inventory_proprietary_summaries_plan_version_pricing_parameters] FOREIGN KEY ([plan_version_pricing_parameter_id]) REFERENCES [dbo].[plan_version_pricing_parameters] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_pricing_parameter_inventory_proprietary_summaries_plan_version_pricing_parameters]
    ON [dbo].[plan_version_pricing_parameter_inventory_proprietary_summaries]([plan_version_pricing_parameter_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_pricing_parameter_inventory_proprietary_summaries_inventory_proprietary_summary]
    ON [dbo].[plan_version_pricing_parameter_inventory_proprietary_summaries]([inventory_proprietary_summary_id] ASC);

