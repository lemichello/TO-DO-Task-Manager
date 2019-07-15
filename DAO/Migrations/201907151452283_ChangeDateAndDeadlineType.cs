namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeDateAndDeadlineType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ToDoItems", "Date", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ToDoItems", "Deadline", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ToDoItems", "Deadline", c => c.String());
            AlterColumn("dbo.ToDoItems", "Date", c => c.String());
        }
    }
}
