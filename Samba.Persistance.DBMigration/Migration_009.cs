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
            Create.Column("Tag").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("UsageCount").OnTable("ScreenMenuItems").AsInt32().WithDefaultValue(0);
            Create.Column("ItemPortion").OnTable("ScreenMenuItems").AsString(128).Nullable();
            Create.Column("SubButtonHeight").OnTable("ScreenMenuCategories").AsInt32().WithDefaultValue(65);
            Create.Column("MaxItems").OnTable("ScreenMenuCategories").AsInt32().WithDefaultValue(0);
            Create.Column("SortType").OnTable("ScreenMenuCategories").AsInt32().WithDefaultValue(0);
            Create.Column("TaxAmount").OnTable("TicketItemProperties").AsDecimal(16, 2).WithDefaultValue(0);

            Create.Column("TaxRate").OnTable("TicketItems").AsDecimal(16, 2).WithDefaultValue(0);
            Create.Column("TaxAmount").OnTable("TicketItems").AsDecimal(16, 2).WithDefaultValue(0);
            Create.Column("TaxTemplateId").OnTable("TicketItems").AsInt32().WithDefaultValue(0);
            Create.Column("TaxIncluded").OnTable("TicketItems").AsBoolean().WithDefaultValue(false);

            Create.Column("TaxTemplate_Id").OnTable("MenuItems").AsInt32().Nullable();

            Create.Table("TaxTemplates")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(128).Nullable()
                .WithColumn("Rate").AsDecimal(16, 2)
                .WithColumn("TaxIncluded").AsBoolean().WithDefaultValue(false);

            Create.ForeignKey("MenuItem_TaxTemplate")
                .FromTable("MenuItems").ForeignColumn("TaxTemplate_Id")
                .ToTable("TaxTemplates").PrimaryColumn("Id");
        }

        public override void Down()
        {
            //do nothing
        }
    }
}
