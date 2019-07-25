namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReplaceProjectIdWithForeignKey : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ToDoItems", "ProjectId");
            AddForeignKey("dbo.ToDoItems", "ProjectId", "dbo.Projects", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ToDoItems", "ProjectId", "dbo.Projects");
            DropIndex("dbo.ToDoItems", new[] { "ProjectId" });
        }
    }
}
