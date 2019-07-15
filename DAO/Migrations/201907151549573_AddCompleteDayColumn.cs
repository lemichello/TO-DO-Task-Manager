namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCompleteDayColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ToDoItems", "CompleteDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ToDoItems", "CompleteDate");
        }
    }
}
