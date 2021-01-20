CREATE TABLE [dbo].[standard_dayparts] (
    [id]                           INT          IDENTITY (1, 1) NOT NULL,
    [daypart_type]                 INT          NOT NULL,
    [daypart_id]                   INT          NOT NULL,
    [code]                         VARCHAR (15) NOT NULL,
    [name]                         VARCHAR (63) NOT NULL,
    [vpvh_calculation_source_type] INT          NOT NULL,
    CONSTRAINT [PK_standard_dayparts] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_daypart_defaults_dayparts1] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_daypart_defaults_dayparts1]
    ON [dbo].[standard_dayparts]([daypart_id] ASC);

