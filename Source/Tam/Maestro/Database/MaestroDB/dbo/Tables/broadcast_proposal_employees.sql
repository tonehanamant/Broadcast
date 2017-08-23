CREATE TABLE [dbo].[broadcast_proposal_employees] (
    [broadcast_proposal_id] INT      NOT NULL,
    [employee_id]           INT      NOT NULL,
    [effective_date]        DATETIME NOT NULL,
    CONSTRAINT [PK_broadcast_proposal_employees] PRIMARY KEY CLUSTERED ([broadcast_proposal_id] ASC, [employee_id] ASC, [effective_date] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_broadcast_proposal_employees_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_broadcast_proposal_employees_proposals] FOREIGN KEY ([broadcast_proposal_id]) REFERENCES [dbo].[broadcast_proposals] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_employees';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_employees';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_employees';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_employees', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_employees', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_employees', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_employees', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_employees', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_employees', @level2type = N'COLUMN', @level2name = N'effective_date';

