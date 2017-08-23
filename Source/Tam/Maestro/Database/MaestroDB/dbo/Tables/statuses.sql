CREATE TABLE [dbo].[statuses] (
    [id]          INT          IDENTITY (1, 1) NOT NULL,
    [status_set]  VARCHAR (15) NOT NULL,
    [name]        VARCHAR (15) NOT NULL,
    [description] VARCHAR (63) NOT NULL,
    CONSTRAINT [PK_statuses] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [unique_statuses] UNIQUE NONCLUSTERED ([status_set] ASC, [name] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Statuses', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Defines a logical grouping of status items.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses', @level2type = N'COLUMN', @level2name = N'status_set';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses', @level2type = N'COLUMN', @level2name = N'status_set';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Name', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Description', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses', @level2type = N'COLUMN', @level2name = N'description';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'statuses', @level2type = N'COLUMN', @level2name = N'description';

