/****** Object:  Table [dbo].[carrot_SerialCache]    Script Date: 08/14/2012 21:59:26 ******/

CREATE TABLE [dbo].[carrot_SerialCache](
	[SerialCacheID] [uniqueidentifier] NOT NULL,
	[SiteID] [uniqueidentifier] NOT NULL,
	[ItemID] [uniqueidentifier] NOT NULL,
	[EditUserId] [uniqueidentifier] NOT NULL,
	[KeyType] [varchar](256) NULL,
	[SerializedData] [varchar](max) NULL,
	[EditDate] [datetime] NOT NULL,
 CONSTRAINT [carrot_SerialCache_PK] PRIMARY KEY CLUSTERED 
(
	[SerialCacheID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[carrot_SerialCache] ADD  CONSTRAINT [DF_carrot_SerialCache_SerialCacheID]  DEFAULT (newid()) FOR [SerialCacheID]
GO
ALTER TABLE [dbo].[carrot_SerialCache] ADD  CONSTRAINT [DF_carrot_SerialCache_EditDate]  DEFAULT (getdate()) FOR [EditDate]
GO
