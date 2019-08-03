namespace DAO.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReplaceBelongsToProjectByProjectId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tags", "ProjectId", c => c.Int());
            CreateIndex("dbo.Tags", "ProjectId");
            AddForeignKey("dbo.Tags", "ProjectId", "dbo.Projects", "Id");
            DropColumn("dbo.Tags", "BelongsToProject");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tags", "BelongsToProject", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.Tags", "ProjectId", "dbo.Projects");
            DropIndex("dbo.Tags", new[] { "ProjectId" });
            DropColumn("dbo.Tags", "ProjectId");
        }
    }
}
