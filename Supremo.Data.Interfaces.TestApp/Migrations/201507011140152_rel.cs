namespace Supremo.Data.Interfaces.TestApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "OrderDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "OrderDate", c => c.DateTime(nullable: false));
        }
    }
}
