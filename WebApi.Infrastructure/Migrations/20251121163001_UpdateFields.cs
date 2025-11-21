using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_completed_tasks_tasks_TaskForTestId",
                table: "completed_tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_test_tasks_tasks_TaskForTestId",
                table: "test_tasks");

            migrationBuilder.RenameColumn(
                name: "TaskForTestId",
                table: "test_tasks",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_test_tasks_TaskForTestId",
                table: "test_tasks",
                newName: "IX_test_tasks_TaskId");

            migrationBuilder.RenameColumn(
                name: "TaskForTestId",
                table: "completed_tasks",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_completed_tasks_TaskForTestId",
                table: "completed_tasks",
                newName: "IX_completed_tasks_TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_completed_tasks_tasks_TaskId",
                table: "completed_tasks",
                column: "TaskId",
                principalTable: "tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_test_tasks_tasks_TaskId",
                table: "test_tasks",
                column: "TaskId",
                principalTable: "tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_completed_tasks_tasks_TaskId",
                table: "completed_tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_test_tasks_tasks_TaskId",
                table: "test_tasks");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "test_tasks",
                newName: "TaskForTestId");

            migrationBuilder.RenameIndex(
                name: "IX_test_tasks_TaskId",
                table: "test_tasks",
                newName: "IX_test_tasks_TaskForTestId");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "completed_tasks",
                newName: "TaskForTestId");

            migrationBuilder.RenameIndex(
                name: "IX_completed_tasks_TaskId",
                table: "completed_tasks",
                newName: "IX_completed_tasks_TaskForTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_completed_tasks_tasks_TaskForTestId",
                table: "completed_tasks",
                column: "TaskForTestId",
                principalTable: "tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_test_tasks_tasks_TaskForTestId",
                table: "test_tasks",
                column: "TaskForTestId",
                principalTable: "tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
