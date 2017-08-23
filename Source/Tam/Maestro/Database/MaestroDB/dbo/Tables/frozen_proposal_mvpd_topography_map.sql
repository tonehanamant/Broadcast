CREATE TABLE [dbo].[frozen_proposal_mvpd_topography_map] (
    [id]                     INT      IDENTITY (1, 1) NOT NULL,
    [media_month_id]         SMALLINT NOT NULL,
    [proposal_topography_id] INT      NOT NULL,
    [mvpd_topography_id]     INT      NOT NULL,
    [mvpd_business_id]       INT      NOT NULL,
    CONSTRAINT [PK_frozen_proposal_mvpd_topography_map] PRIMARY KEY CLUSTERED ([id] ASC)
);

