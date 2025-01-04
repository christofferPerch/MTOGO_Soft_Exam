USE [MASTER]
GO
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'OrderServiceTestDB')
BEGIN
    CREATE DATABASE OrderServiceTestDB;
END
GO
USE [OrderServiceTestDB];
GO
/****** Object:  UserDefinedTableType [dbo].[TVP_OrderItem]    Script Date: 02-12-2024 16:36:58 ******/
CREATE TYPE [dbo].[TVP_OrderItem] AS TABLE(
	[RestaurantId] [int] NOT NULL,
	[MenuItemId] [int] NOT NULL,
	[Price] [decimal](10, 2) NOT NULL,
	[Quantity] [int] NOT NULL
)
GO
/****** Object:  Table [dbo].[Order]    Script Date: 02-12-2024 16:36:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](300) NULL,
	[DeliveryAgentId] [int] NULL,
	[TotalAmount] [decimal](10, 2) NOT NULL,
	[VATAmount] [decimal](10, 2) NOT NULL,
	[OrderPlacedTimestamp] [datetime] NOT NULL,
	[OrderStatusId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItem]    Script Date: 02-12-2024 16:36:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[RestaurantId] [int] NOT NULL,
	[MenuItemId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[Price] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Order] ADD  DEFAULT (getdate()) FOR [OrderPlacedTimestamp]
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD  CONSTRAINT [FK_OrderItem_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Order] ([Id])
GO
ALTER TABLE [dbo].[OrderItem] CHECK CONSTRAINT [FK_OrderItem_Order]
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD CHECK  (([Quantity]>(0)))
GO
/****** Object:  StoredProcedure [dbo].[AddOrder]    Script Date: 02-12-2024 16:36:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddOrder]
    @UserId nvarchar(300),
    @TotalAmount DECIMAL(10, 2),
    @VATAmount DECIMAL(10, 2),
    @OrderPlacedTimestamp DATETIME,
    @OrderStatusId INT,
    @OrderItems TVP_OrderItem READONLY,  -- Table-Valued Parameter for Order Items
    @OrderId INT OUTPUT                  -- Output parameter for the new Order ID
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Order table and get the generated OrderId
        INSERT INTO [Order] (UserId, TotalAmount, VATAmount, OrderPlacedTimestamp, OrderStatusId)
        VALUES (@UserId, @TotalAmount, @VATAmount, @OrderPlacedTimestamp, @OrderStatusId);

        SET @OrderId = SCOPE_IDENTITY();

        -- Insert related Order Items, each with its own RestaurantId
        INSERT INTO OrderItem (OrderId, RestaurantId, MenuItemId, Price, Quantity)
        SELECT @OrderId, RestaurantId, MenuItemId, Price, Quantity
        FROM @OrderItems;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
