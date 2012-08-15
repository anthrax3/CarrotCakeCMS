/****** Object:  Table [dbo].[carrot_Content]    Script Date: 08/14/2012 21:59:26 ******/

CREATE TABLE [dbo].[carrot_Content](
	[ContentID] [uniqueidentifier] NOT NULL,
	[Root_ContentID] [uniqueidentifier] NOT NULL,
	[Parent_ContentID] [uniqueidentifier] NULL,
	[IsLatestVersion] [bit] NULL,
	[TitleBar] [varchar](256) NULL,
	[NavMenuText] [varchar](256) NULL,
	[PageHead] [varchar](256) NULL,
	[PageText] [varchar](max) NULL,
	[LeftPageText] [varchar](max) NULL,
	[RightPageText] [varchar](max) NULL,
	[NavOrder] [int] NULL,
	[EditUserId] [uniqueidentifier] NULL,
	[EditDate] [datetime] NOT NULL,
	[TemplateFile] [nvarchar](256) NULL,
	[MetaKeyword] [varchar](1000) NULL,
	[MetaDescription] [varchar](2000) NULL,
 CONSTRAINT [PK_carrot_Content] PRIMARY KEY CLUSTERED 
(
	[ContentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[carrot_Content]  WITH CHECK ADD  CONSTRAINT [carrot_Content_EditUserId_FK] FOREIGN KEY([EditUserId])
REFERENCES [dbo].[aspnet_Users] ([UserId])
GO
ALTER TABLE [dbo].[carrot_Content] CHECK CONSTRAINT [carrot_Content_EditUserId_FK]
GO
ALTER TABLE [dbo].[carrot_Content]  WITH CHECK ADD  CONSTRAINT [carrot_RootContent_carrot_Content_FK] FOREIGN KEY([Root_ContentID])
REFERENCES [dbo].[carrot_RootContent] ([Root_ContentID])
GO
ALTER TABLE [dbo].[carrot_Content] CHECK CONSTRAINT [carrot_RootContent_carrot_Content_FK]
GO
ALTER TABLE [dbo].[carrot_Content] ADD  CONSTRAINT [DF_carrot_Content_ContentID]  DEFAULT (newid()) FOR [ContentID]
GO
ALTER TABLE [dbo].[carrot_Content] ADD  CONSTRAINT [DF_carrot_Content_EditDate]  DEFAULT (getdate()) FOR [EditDate]
GO
