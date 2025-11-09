-- Create Properties Table for REM System
-- This table stores real estate properties and products listed by sellers

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Properties' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Properties] (
        [PropertyId] INT IDENTITY(1,1) PRIMARY KEY,
        [SellerId] INT NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [PropertyType] NVARCHAR(50) NOT NULL, -- 'RealEstate' or 'Product'
        [Address] NVARCHAR(500) NULL,
        [Price] DECIMAL(18, 2) NOT NULL,
        [Bedrooms] INT NULL,
        [Bathrooms] INT NULL,
        [Area] DECIMAL(18, 2) NULL, -- Square feet or square meters
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Available', -- 'Available', 'Sold', 'Pending'
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [ModifiedDate] DATETIME NULL,
        
        -- Foreign Key constraint to link with Users table
        CONSTRAINT [FK_Properties_Users] FOREIGN KEY ([SellerId]) 
            REFERENCES [dbo].[Users]([UserId]) ON DELETE CASCADE
    );

    -- Create index on SellerId for faster queries
    CREATE INDEX [IX_Properties_SellerId] ON [dbo].[Properties]([SellerId]);
    
    -- Create index on PropertyType for filtering
    CREATE INDEX [IX_Properties_PropertyType] ON [dbo].[Properties]([PropertyType]);
    
    -- Create index on Status for filtering
    CREATE INDEX [IX_Properties_Status] ON [dbo].[Properties]([Status]);

    PRINT 'Properties table created successfully.';
END
ELSE
BEGIN
    PRINT 'Properties table already exists.';
END
GO

-- Verify Users table has UserId column (required for foreign key)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'UserId')
BEGIN
    PRINT 'ERROR: Users table does not have UserId column. Please ensure Users table is properly created with UserId as primary key.';
END
ELSE
BEGIN
    PRINT 'Users table verified. Foreign key relationship can be established.';
END
GO

