using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPark.WorkOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "work_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BusinessType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ParkArea = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RelatedAssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedAlertId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReporterName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DispatcherUserId = table.Column<string>(type: "text", nullable: true),
                    DispatcherName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AssigneeUserId = table.Column<string>(type: "text", nullable: true),
                    AssigneeName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReviewerUserId = table.Column<string>(type: "text", nullable: true),
                    ReviewerName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerificationNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CloseNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArrivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "work_order_action_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OperatorUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OperatorName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_action_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_order_action_logs_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_order_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_order_attachments_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_action_logs_WorkOrderId",
                table: "work_order_action_logs",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_attachments_WorkOrderId",
                table: "work_order_attachments",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_Number",
                table: "work_orders",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "work_order_action_logs");

            migrationBuilder.DropTable(
                name: "work_order_attachments");

            migrationBuilder.DropTable(
                name: "work_orders");
        }
    }
}
