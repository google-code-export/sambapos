using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(9)]
    public class Migration_009 : Migration
    {
        public override void Up()
        {
            Create.Column("ExcludeVat").OnTable("PrintJobs").AsBoolean().WithDefaultValue(false);

            Create.Column("Tag").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("UsageCount").OnTable("ScreenMenuItems").AsInt32().WithDefaultValue(0);
            Create.Column("ItemPortion").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("SubButtonHeight").OnTable("ScreenMenuCategories").AsInt32().WithDefaultValue(65);
            Create.Column("MaxItems").OnTable("ScreenMenuCategories").AsInt32().WithDefaultValue(0);
            Create.Column("SortType").OnTable("ScreenMenuCategories").AsInt32().WithDefaultValue(0);
            Create.Column("VatAmount").OnTable("TicketItemProperties").AsDecimal(16, 2).WithDefaultValue(0);
            
            Create.Column("VatRate").OnTable("TicketItems").AsDecimal(16, 2).WithDefaultValue(0);
            Create.Column("VatAmount").OnTable("TicketItems").AsDecimal(16, 2).WithDefaultValue(0);
            Create.Column("VatTemplateId").OnTable("TicketItems").AsInt32().WithDefaultValue(0);
            Create.Column("VatIncluded").OnTable("TicketItems").AsBoolean().WithDefaultValue(false);

            Create.Column("VatTemplate_Id").OnTable("MenuItems").AsInt32().Nullable();

            Create.Table("VatTemplates")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(128).Nullable()
                .WithColumn("Rate").AsDecimal(16, 2)
                .WithColumn("VatIncluded").AsBoolean().WithDefaultValue(false);

            Create.ForeignKey("MenuItem_VatTemplate")
                .FromTable("MenuItems").ForeignColumn("VatTemplate_Id")
                .ToTable("VatTemplates").PrimaryColumn("Id");
        }

        public override void Down()
        {
            //do nothing
        }
    }
}
