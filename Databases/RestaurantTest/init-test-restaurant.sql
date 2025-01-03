USE [MASTER]
GO
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RestaurantServiceTestDB')
BEGIN
    CREATE DATABASE RestaurantServiceTestDB;
END
GO
USE [RestaurantServiceTestDB];
GO
/****** Object:  UserDefinedTableType [dbo].[TVP_FoodCategory]    Script Date: 02-12-2024 16:38:49 ******/
CREATE TYPE [dbo].[TVP_FoodCategory] AS TABLE(
	[Category] [int] NOT NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TVP_MenuItem]    Script Date: 02-12-2024 16:38:49 ******/
CREATE TYPE [dbo].[TVP_MenuItem] AS TABLE(
	[Name] [nvarchar](100) NULL,
	[Description] [nvarchar](255) NULL,
	[Price] [decimal](10, 2) NULL,
	[Image] [varbinary](max) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TVP_OperatingHours]    Script Date: 02-12-2024 16:38:49 ******/
CREATE TYPE [dbo].[TVP_OperatingHours] AS TABLE(
	[Day] [int] NULL,
	[OpeningHours] [time](7) NULL,
	[ClosingHours] [time](7) NULL
)
GO
/****** Object:  Table [dbo].[Address]    Script Date: 02-12-2024 16:38:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Address](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AddressLine1] [nvarchar](255) NOT NULL,
	[AddressLine2] [nvarchar](255) NULL,
	[City] [nvarchar](100) NOT NULL,
	[ZipCode] [nvarchar](20) NOT NULL,
	[Country] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FoodCategory]    Script Date: 02-12-2024 16:38:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoodCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RestaurantId] [int] NOT NULL,
	[Category] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MenuItem]    Script Date: 02-12-2024 16:38:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MenuItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RestaurantId] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[Price] [decimal](10, 2) NOT NULL,
	[Image] [varbinary](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OperatingHours]    Script Date: 02-12-2024 16:38:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OperatingHours](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RestaurantId] [int] NOT NULL,
	[Day] [int] NOT NULL,
	[OpeningHours] [time](7) NOT NULL,
	[ClosingHours] [time](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Restaurant]    Script Date: 02-12-2024 16:38:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Restaurant](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RestaurantName] [nvarchar](100) NOT NULL,
	[LegalName] [nvarchar](255) NOT NULL,
	[VATNumber] [nvarchar](50) NOT NULL,
	[RestaurantDescription] [nvarchar](500) NULL,
	[ContactEmail] [nvarchar](255) NOT NULL,
	[ContactPhone] [nvarchar](50) NOT NULL,
	[AddressId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[FoodCategory]  WITH CHECK ADD  CONSTRAINT [FK_FoodCategory_Restaurant] FOREIGN KEY([RestaurantId])
REFERENCES [dbo].[Restaurant] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FoodCategory] CHECK CONSTRAINT [FK_FoodCategory_Restaurant]
GO
ALTER TABLE [dbo].[MenuItem]  WITH CHECK ADD FOREIGN KEY([RestaurantId])
REFERENCES [dbo].[Restaurant] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OperatingHours]  WITH CHECK ADD FOREIGN KEY([RestaurantId])
REFERENCES [dbo].[Restaurant] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Restaurant]  WITH CHECK ADD FOREIGN KEY([AddressId])
REFERENCES [dbo].[Address] ([Id])
ON DELETE CASCADE
GO
/****** Object:  StoredProcedure [dbo].[AddRestaurant]    Script Date: 02-12-2024 16:38:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddRestaurant]
    @RestaurantName NVARCHAR(100),
    @LegalName NVARCHAR(255),
    @VATNumber NVARCHAR(50),
    @RestaurantDescription NVARCHAR(500),
    @ContactEmail NVARCHAR(255),
    @ContactPhone NVARCHAR(50),
    @AddressLine1 NVARCHAR(255),
    @AddressLine2 NVARCHAR(255),
    @City NVARCHAR(100),
    @ZipCode NVARCHAR(20),
    @Country NVARCHAR(100),
    @OperatingHours TVP_OperatingHours READONLY,  -- Table-valued parameter for OperatingHours
    @FoodCategories TVP_FoodCategory READONLY,    -- Table-valued parameter for FoodCategories
    @RestaurantId INT OUTPUT                      -- Output parameter for the new Restaurant ID
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Address and get AddressId
        DECLARE @AddressId INT;
        INSERT INTO Address (AddressLine1, AddressLine2, City, ZipCode, Country)
        VALUES (@AddressLine1, @AddressLine2, @City, @ZipCode, @Country);
        SET @AddressId = SCOPE_IDENTITY();

        -- Insert into Restaurant and get RestaurantId
        INSERT INTO Restaurant (RestaurantName, LegalName, VATNumber, RestaurantDescription, ContactEmail, ContactPhone, AddressId)
        VALUES (@RestaurantName, @LegalName, @VATNumber, @RestaurantDescription, @ContactEmail, @ContactPhone, @AddressId);
        SET @RestaurantId = SCOPE_IDENTITY();

        -- Insert OperatingHours for each day
        INSERT INTO OperatingHours (RestaurantId, Day, OpeningHours, ClosingHours)
        SELECT @RestaurantId, Day, OpeningHours, ClosingHours
        FROM @OperatingHours;

        -- Insert FoodCategories for the restaurant
        INSERT INTO FoodCategory (RestaurantId, Category)
        SELECT @RestaurantId, Category
        FROM @FoodCategories;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[UpdateRestaurant]    Script Date: 02-12-2024 16:38:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateRestaurant]
    @RestaurantId INT,
    @RestaurantName NVARCHAR(100) = NULL,
    @LegalName NVARCHAR(255) = NULL,
    @VATNumber NVARCHAR(50) = NULL,
    @RestaurantDescription NVARCHAR(MAX) = NULL,
    @ContactEmail NVARCHAR(255) = NULL,
    @ContactPhone NVARCHAR(50) = NULL,
    @AddressLine1 NVARCHAR(255) = NULL,
    @AddressLine2 NVARCHAR(255) = NULL,
    @City NVARCHAR(100) = NULL,
    @ZipCode NVARCHAR(20) = NULL,
    @Country NVARCHAR(100) = NULL,
    @OperatingHours TVP_OperatingHours READONLY,    -- Table-valued parameter for OperatingHours
    @FoodCategories TVP_FoodCategory READONLY       -- Table-valued parameter for FoodCategories
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RowsAffected INT = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Update Restaurant table
        UPDATE Restaurant
        SET
            RestaurantName = COALESCE(@RestaurantName, RestaurantName),
            LegalName = COALESCE(@LegalName, LegalName),
            VATNumber = COALESCE(@VATNumber, VATNumber),
            RestaurantDescription = COALESCE(@RestaurantDescription, RestaurantDescription),
            ContactEmail = COALESCE(@ContactEmail, ContactEmail),
            ContactPhone = COALESCE(@ContactPhone, ContactPhone)
        WHERE Id = @RestaurantId;
        
        SET @RowsAffected = @RowsAffected + @@ROWCOUNT;

        -- Update Address table conditionally
        UPDATE a
        SET
            AddressLine1 = COALESCE(@AddressLine1, AddressLine1),
            AddressLine2 = COALESCE(@AddressLine2, AddressLine2),
            City = COALESCE(@City, City),
            ZipCode = COALESCE(@ZipCode, ZipCode),
            Country = COALESCE(@Country, Country)
        FROM Address a
        INNER JOIN Restaurant r ON r.AddressId = a.Id
        WHERE r.Id = @RestaurantId;
        
        SET @RowsAffected = @RowsAffected + @@ROWCOUNT;

        -- Delete existing OperatingHours entries
        DELETE FROM OperatingHours WHERE RestaurantId = @RestaurantId;

        -- Insert updated OperatingHours entries
        INSERT INTO OperatingHours (RestaurantId, Day, OpeningHours, ClosingHours)
        SELECT @RestaurantId, Day, OpeningHours, ClosingHours
        FROM @OperatingHours;

        -- Delete existing FoodCategories for the restaurant
        DELETE FROM FoodCategory WHERE RestaurantId = @RestaurantId;

        -- Insert updated FoodCategories
        INSERT INTO FoodCategory (RestaurantId, Category)
        SELECT @RestaurantId, Category
        FROM @FoodCategories;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
