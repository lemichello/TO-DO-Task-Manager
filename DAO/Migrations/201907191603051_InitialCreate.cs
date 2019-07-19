namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItemTags",
                c => new
                    {
                        ItemId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItemId, t.TagId })
                .ForeignKey("dbo.ToDoItems", t => t.ItemId, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagId, cascadeDelete: true)
                .Index(t => t.ItemId)
                .Index(t => t.TagId);
            
            CreateTable(
                "dbo.ToDoItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Header = c.String(nullable: false),
                        Notes = c.String(),
                        Date = c.DateTime(nullable: false),
                        Deadline = c.DateTime(nullable: false),
                        CompleteDate = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Login = c.String(nullable: false),
                        Password = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: false)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItemTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.ItemTags", "ItemId", "dbo.ToDoItems");
            DropForeignKey("dbo.ToDoItems", "UserId", "dbo.Users");
            DropForeignKey("dbo.Tags", "UserId", "dbo.Users");
            DropIndex("dbo.Tags", new[] { "UserId" });
            DropIndex("dbo.ToDoItems", new[] { "UserId" });
            DropIndex("dbo.ItemTags", new[] { "TagId" });
            DropIndex("dbo.ItemTags", new[] { "ItemId" });
            DropTable("dbo.Tags");
            DropTable("dbo.Users");
            DropTable("dbo.ToDoItems");
            DropTable("dbo.ItemTags");
        }
    }
}
