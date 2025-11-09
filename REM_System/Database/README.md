# Database Setup Instructions

## Prerequisites
- SQL Server Express (or higher) must be installed
- The database connection string in `App.config` must point to your SQL Server instance

## Setup Steps

1. **Create the Database** (if not already created):
   ```sql
   CREATE DATABASE RealEstate;
   GO
   USE RealEstate;
   GO
   ```

2. **Create the Users Table** (if not already created):
   ```sql
   CREATE TABLE [dbo].[Users] (
       [UserId] INT IDENTITY(1,1) PRIMARY KEY,
       [Username] NVARCHAR(100) NOT NULL UNIQUE,
       [Password] NVARCHAR(256) NOT NULL,
       [Email] NVARCHAR(256) NOT NULL UNIQUE,
       [UserRole] NVARCHAR(50) NOT NULL DEFAULT 'User'
   );
   GO
   ```

3. **Create the Properties Table**:
   - Run the script `CreatePropertiesTable.sql` in SQL Server Management Studio
   - Or execute it via sqlcmd or your preferred SQL tool
   - This will create the Properties table with all necessary columns and indexes

4. **Verify the Setup**:
   - Check that both tables exist: `Users` and `Properties`
   - Verify that the foreign key relationship is established between Properties.SellerId and Users.UserId

## Table Structure

### Properties Table
- **PropertyId**: Primary key, auto-increment
- **SellerId**: Foreign key to Users table
- **Title**: Property/product title (required)
- **Description**: Detailed description (optional)
- **PropertyType**: Type of listing - 'RealEstate' or 'Product' (required)
- **Address**: Property address (optional)
- **Price**: Price in decimal format (required)
- **Bedrooms**: Number of bedrooms (optional, for real estate)
- **Bathrooms**: Number of bathrooms (optional, for real estate)
- **Area**: Area in square feet/meters (optional)
- **Status**: Current status - 'Available', 'Sold', or 'Pending' (required)
- **CreatedDate**: Timestamp when property was created
- **ModifiedDate**: Timestamp when property was last modified

## Notes
- The Properties table has a foreign key constraint that links to the Users table
- When a user (seller) is deleted, their properties will be automatically deleted (CASCADE)
- Indexes are created on SellerId, PropertyType, and Status for better query performance

