namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProjectIdForToDoItemTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ToDoItems", "ProjectId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ToDoItems", "ProjectId");
        }
    }
}
