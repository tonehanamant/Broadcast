CREATE TABLE [dbo].[dayparts] (
    [id]           INT          IDENTITY (1, 1) NOT NULL,
    [timespan_id]  INT          NOT NULL,
    [code]         VARCHAR (15) NOT NULL,
    [name]         VARCHAR (63) NOT NULL,
    [tier]         INT          NOT NULL,
    [daypart_text] VARCHAR (63) NOT NULL,
    [total_hours]  FLOAT (53)   NOT NULL,
    CONSTRAINT [PK_dayparts] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_dayparts_timespans] FOREIGN KEY ([timespan_id]) REFERENCES [dbo].[timespans] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_dayparts_timespans]
    ON [dbo].[dayparts]([timespan_id] ASC);

