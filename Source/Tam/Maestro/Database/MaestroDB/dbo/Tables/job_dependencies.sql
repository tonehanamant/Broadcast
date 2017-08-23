CREATE TABLE [dbo].[job_dependencies] (
    [predicate_job_id] INT NOT NULL,
    [dependent_job_id] INT NOT NULL,
    CONSTRAINT [PK_job_dependencies] PRIMARY KEY CLUSTERED ([predicate_job_id] ASC, [dependent_job_id] ASC),
    CONSTRAINT [FK_dependent_job_dependency_2_job] FOREIGN KEY ([dependent_job_id]) REFERENCES [dbo].[jobs] ([id]),
    CONSTRAINT [FK_predicate_job_dependency_2_job] FOREIGN KEY ([predicate_job_id]) REFERENCES [dbo].[jobs] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'job_dependencies';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'job_dependencies';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'job_dependencies';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'job_dependencies', @level2type = N'COLUMN', @level2name = N'predicate_job_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'job_dependencies', @level2type = N'COLUMN', @level2name = N'predicate_job_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'job_dependencies', @level2type = N'COLUMN', @level2name = N'dependent_job_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'job_dependencies', @level2type = N'COLUMN', @level2name = N'dependent_job_id';

