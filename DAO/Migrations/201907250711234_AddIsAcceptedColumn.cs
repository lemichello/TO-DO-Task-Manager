namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsAcceptedColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectsUsers", "IsAccepted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectsUsers", "IsAccepted");
        }
    }
}
