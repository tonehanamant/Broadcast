CREATE TABLE [dbo].[traffic_dollars_allocation_lookup] (
    [proposal_id]                INT   NOT NULL,
    [traffic_id]                 INT   NOT NULL,
    [proposal_dollars_allocated] MONEY CONSTRAINT [DF_traffic_dollars_allocation_lookup_proposal_dollars_allocated] DEFAULT ((0.0)) NOT NULL,
    [traffic_dollars_allocated]  MONEY CONSTRAINT [DF_traffic_dollars_allocation_lookup_traffic_dollars_allocated] DEFAULT ((0.0)) NOT NULL,
    [release_dollars_allocated]  MONEY CONSTRAINT [DF_traffic_dollars_allocation_lookup_release_dollars_allocated] DEFAULT ((0.0)) NOT NULL,
    CONSTRAINT [PK_traffic_dollars_allocation_lookup] PRIMARY KEY CLUSTERED ([proposal_id] ASC, [traffic_id] ASC) WITH (FILLFACTOR = 90)
);

