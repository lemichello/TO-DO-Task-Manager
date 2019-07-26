namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInviterColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectsUsers", "InviterId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProjectsUsers", "InviterId");
            AddForeignKey("dbo.ProjectsUsers", "InviterId", "dbo.Users", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProjectsUsers", "InviterId", "dbo.Users");
            DropIndex("dbo.ProjectsUsers", new[] { "InviterId" });
            DropColumn("dbo.ProjectsUsers", "InviterId");
        }
    }
}
