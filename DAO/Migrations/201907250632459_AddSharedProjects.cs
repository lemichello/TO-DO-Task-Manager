namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSharedProjects : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProjectsUsers",
                c => new
                    {
                        ProjectId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ProjectId, t.UserId })
                .ForeignKey("dbo.Projects", t => t.ProjectId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ProjectId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProjectsUsers", "UserId", "dbo.Users");
            DropForeignKey("dbo.ProjectsUsers", "ProjectId", "dbo.Projects");
            DropIndex("dbo.ProjectsUsers", new[] { "UserId" });
            DropIndex("dbo.ProjectsUsers", new[] { "ProjectId" });
            DropTable("dbo.ProjectsUsers");
            DropTable("dbo.Projects");
        }
    }
}
