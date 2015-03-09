namespace Supremo.Data.Interfaces.TestApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderItem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Value = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        UpdatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(),
                        Order_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.Order_Id)
                .Index(t => t.Order_Id);
            
            AddColumn("dbo.Orders", "OrderDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Orders", "Value");
            DropColumn("dbo.Orders", "IsDeleted");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "IsDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Orders", "Value", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropForeignKey("dbo.OrderItems", "Order_Id", "dbo.Orders");
            DropIndex("dbo.OrderItems", new[] { "Order_Id" });
            DropColumn("dbo.Orders", "OrderDate");
            DropTable("dbo.OrderItems");
        }
    }
}
