namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRequiredAttributeFromNotes : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ToDoItems", "Notes", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ToDoItems", "Notes", c => c.String(nullable: false));
        }
    }
}
