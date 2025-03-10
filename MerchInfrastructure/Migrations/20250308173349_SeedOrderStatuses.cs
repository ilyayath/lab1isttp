using Microsoft.EntityFrameworkCore.Migrations;

namespace MerchInfrastructure.Migrations
{
    public partial class SeedOrderStatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO \"OrderStatuses\" (\"StatusName\") VALUES ('Нове') ON CONFLICT DO NOTHING;");
            migrationBuilder.Sql("INSERT INTO \"OrderStatuses\" (\"StatusName\") VALUES ('В обробці') ON CONFLICT DO NOTHING;");
            migrationBuilder.Sql("INSERT INTO \"OrderStatuses\" (\"StatusName\") VALUES ('Відправлено') ON CONFLICT DO NOTHING;");
            migrationBuilder.Sql("INSERT INTO \"OrderStatuses\" (\"StatusName\") VALUES ('Доставлено') ON CONFLICT DO NOTHING;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"OrderStatuses\" WHERE \"StatusName\" IN ('Нове', 'В обробці', 'Відправлено', 'Доставлено');");
        }
    }
}