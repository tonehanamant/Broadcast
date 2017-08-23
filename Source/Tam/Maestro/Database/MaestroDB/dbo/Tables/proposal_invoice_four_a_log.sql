CREATE TABLE [dbo].[proposal_invoice_four_a_log] (
    [proposal_id]           INT      NOT NULL,
    [receivable_invoice_id] INT      NOT NULL,
    [employee_id]           INT      NOT NULL,
    [exported_date]         DATETIME CONSTRAINT [DF_proposal_invoice_four_a_log_transmitted_date] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_proposal_invoice_four_a_log] PRIMARY KEY CLUSTERED ([proposal_id] ASC, [receivable_invoice_id] ASC, [employee_id] ASC, [exported_date] ASC),
    CONSTRAINT [FK_proposal_invoice_four_a_log_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_proposal_invoice_four_a_log_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id])
);

