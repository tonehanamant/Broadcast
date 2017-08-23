CREATE TABLE [dbo].[proposal_employees] (
    [proposal_id]          INT           NOT NULL,
    [employee_id]          INT           NOT NULL,
    [effective_date]       DATETIME      NOT NULL,
    [flight_text]          VARCHAR (MAX) CONSTRAINT [DF_proposal_employees_flight_text] DEFAULT ('') NOT NULL,
    [original_flight_text] VARCHAR (MAX) CONSTRAINT [DF_proposal_employees_original_flight_text] DEFAULT ('') NOT NULL,
    [total_gross_cost]     MONEY         CONSTRAINT [DF_proposal_employees_total_gross_cost] DEFAULT ((0)) NOT NULL,
    [total_rate_card_cost] MONEY         CONSTRAINT [DF_proposal_employees_total_rate_card_cost] DEFAULT ((0)) NOT NULL,
    [total_units]          INT           CONSTRAINT [DF_proposal_employees_total_units] DEFAULT ((0)) NOT NULL,
    [total_detail_lines]   SMALLINT      CONSTRAINT [DF_proposal_employees_total_detail_lines] DEFAULT ((0)) NOT NULL,
    [total_demographics]   TINYINT       CONSTRAINT [DF_proposal_employees_total_demographics] DEFAULT ((0)) NOT NULL,
    [total_hh_delivery]    FLOAT (53)    CONSTRAINT [DF_proposal_employees_hh_delivery] DEFAULT ((0)) NOT NULL,
    [total_hh_cpm]         MONEY         CONSTRAINT [DF_proposal_employees_total_hh_cpm] DEFAULT ((0)) NOT NULL,
    [primary_audience_id]  INT           NULL,
    [total_demo_delivery]  FLOAT (53)    NULL,
    [total_demo_cpm]       MONEY         NULL,
    CONSTRAINT [PK_proposal_employees] PRIMARY KEY CLUSTERED ([proposal_id] ASC, [employee_id] ASC, [effective_date] ASC),
    CONSTRAINT [FK_proposal_employees_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_proposal_employees_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'flight_text';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'flight_text';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'original_flight_text';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'original_flight_text';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_gross_cost';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_gross_cost';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_rate_card_cost';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_rate_card_cost';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_units';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_units';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_detail_lines';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_detail_lines';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_demographics';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_demographics';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_hh_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_hh_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_hh_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_hh_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'primary_audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'primary_audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_demo_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_demo_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_demo_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_employees', @level2type = N'COLUMN', @level2name = N'total_demo_cpm';

