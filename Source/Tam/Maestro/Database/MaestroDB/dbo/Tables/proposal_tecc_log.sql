CREATE TABLE [dbo].[proposal_tecc_log] (
    [proposal_id]      INT      NOT NULL,
    [employee_id]      INT      NOT NULL,
    [transmitted_date] DATETIME CONSTRAINT [DF_proposal_tecc_log_transmitted_date] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_proposal_tecc_log] PRIMARY KEY CLUSTERED ([proposal_id] ASC, [employee_id] ASC, [transmitted_date] ASC),
    CONSTRAINT [FK_proposal_tecc_log_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_proposal_tecc_log_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id])
);

