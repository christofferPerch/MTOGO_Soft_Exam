USE [MASTER]
GO
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ReviewServiceTestDB')
BEGIN
    CREATE DATABASE ReviewServiceTestDB;
END
GO
USE [ReviewServiceTestDB];
GO
/****** Object:  Table [dbo].[RestaurantReview]    Script Date: 02-12-2024 16:39:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RestaurantReview](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [nvarchar](50) NOT NULL,
	[FoodRating] [int] NOT NULL,
	[Comments] [nvarchar](1000) NULL,
	[ReviewTimestamp] [datetime] NOT NULL,
	[RestaurantId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
