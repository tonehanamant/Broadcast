CREATE TABLE [dbo].[zone_states] (
    [zone_id]        INT        NOT NULL,
    [state_id]       INT        NOT NULL,
    [weight]         FLOAT (53) NOT NULL,
    [effective_date] DATETIME   NOT NULL,
    CONSTRAINT [PK_zone_states] PRIMARY KEY CLUSTERED ([zone_id] ASC, [state_id] ASC),
    CONSTRAINT [FK_zone_states_states] FOREIGN KEY ([state_id]) REFERENCES [dbo].[states] ([id]),
    CONSTRAINT [FK_zone_states_zones] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Zones to States', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states', @level2type = N'COLUMN', @level2name = N'state_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states', @level2type = N'COLUMN', @level2name = N'state_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states', @level2type = N'COLUMN', @level2name = N'weight';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states', @level2type = N'COLUMN', @level2name = N'weight';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_states', @level2type = N'COLUMN', @level2name = N'effective_date';

