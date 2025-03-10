using Microsoft.EntityFrameworkCore.Migrations;

namespace MerchInfrastructure.Migrations
{
    public partial class SeedShipments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO \"Shipments\" (\"TypeShipment\") VALUES ('Нова Пошта') ON CONFLICT DO NOTHING;");
            migrationBuilder.Sql("INSERT INTO \"Shipments\" (\"TypeShipment\") VALUES ('Укрпошта') ON CONFLICT DO NOTHING;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"Shipments\" WHERE \"TypeShipment\" IN ('Нова Пошта', 'Укрпошта');");
        }
    }
}