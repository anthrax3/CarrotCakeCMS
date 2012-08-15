/****** Object:  Table [dbo].[carrot_RootContent]    Script Date: 08/14/2012 21:59:26 ******/

CREATE TABLE [dbo].[carrot_RootContent](
	[Root_ContentID] [uniqueidentifier] NOT NULL,
	[SiteID] [uniqueidentifier] NOT NULL,
	[Heartbeat_UserId] [uniqueidentifier] NULL,
	[EditHeartbeat] [datetime] NULL,
	[FileName] [varchar](256) NOT NULL,
	[PageActive] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [carrot_RootContent_PK] PRIMARY KEY CLUSTERED 
(
	[Root_ContentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[carrot_RootContent]  WITH CHECK ADD  CONSTRAINT [carrot_Sites_carrot_RootContent_FK] FOREIGN KEY([SiteID])
REFERENCES [dbo].[carrot_Sites] ([SiteID])
GO
ALTER TABLE [dbo].[carrot_RootContent] CHECK CONSTRAINT [carrot_Sites_carrot_RootContent_FK]
GO
ALTER TABLE [dbo].[carrot_RootContent] ADD  CONSTRAINT [DF_carrot_RootContent_Root_ContentID]  DEFAULT (newid()) FOR [Root_ContentID]
GO
ALTER TABLE [dbo].[carrot_RootContent] ADD  CONSTRAINT [DF_carrot_RootContent_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
