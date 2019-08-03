namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBelongsToProjectColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tags", "BelongsToProject", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tags", "BelongsToProject");
        }
    }
}
