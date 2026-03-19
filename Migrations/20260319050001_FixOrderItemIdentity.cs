using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWD392_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderItemIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        -- 1. Tạo bảng mới với IDENTITY đúng
        CREATE TABLE OrderItems_New (
            OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
            OrderId     INT NOT NULL,
            MenuItemId  INT NOT NULL,
            ItemName    NVARCHAR(255) NOT NULL,
            Quantity    INT NOT NULL,
            UnitPrice   DECIMAL(18,2) NOT NULL
        );

        -- 2. Copy data từ bảng cũ (bỏ qua OrderItemId cũ, để IDENTITY tự generate)
        INSERT INTO OrderItems_New (OrderId, MenuItemId, ItemName, Quantity, UnitPrice)
        SELECT OrderId, MenuItemId, ItemName, Quantity, UnitPrice
        FROM OrderItems;

        -- 3. Drop bảng cũ
        DROP TABLE OrderItems;

        -- 4. Rename bảng mới
        EXEC sp_rename 'OrderItems_New', 'OrderItems';
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE TABLE OrderItems_Old (
            OrderItemId INT PRIMARY KEY,
            OrderId     INT NOT NULL,
            MenuItemId  INT NOT NULL,
            ItemName    NVARCHAR(255) NOT NULL,
            Quantity    INT NOT NULL,
            UnitPrice   DECIMAL(18,2) NOT NULL
        );

        INSERT INTO OrderItems_Old (OrderItemId, OrderId, MenuItemId, ItemName, Quantity, UnitPrice)
        SELECT OrderItemId, OrderId, MenuItemId, ItemName, Quantity, UnitPrice
        FROM OrderItems;

        DROP TABLE OrderItems;

        EXEC sp_rename 'OrderItems_Old', 'OrderItems';
    ");
        }
    }
}
