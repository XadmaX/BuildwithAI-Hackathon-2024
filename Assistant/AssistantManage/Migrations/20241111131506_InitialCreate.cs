using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantManage.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SizeInBytes = table.Column<int>(type: "int", nullable: false),
                    ExternalSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InModelId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VectorStores",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InModelId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VectorStores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YourEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YourEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assistants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, nullable: false),
                    InModelId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Base64Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Temperature = table.Column<float>(type: "real", nullable: false),
                    HasFileSearch = table.Column<bool>(type: "bit", nullable: false),
                    HasCodeInterpreter = table.Column<bool>(type: "bit", nullable: false),
                    Tools = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<string>(type: "varchar(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assistants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assistants_VectorStores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "VectorStores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VectorStoreEntityVectoredFileEntity",
                columns: table => new
                {
                    FilesId = table.Column<string>(type: "varchar(36)", nullable: false),
                    VectorStoreEntityId = table.Column<string>(type: "varchar(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VectorStoreEntityVectoredFileEntity", x => new { x.FilesId, x.VectorStoreEntityId });
                    table.ForeignKey(
                        name: "FK_VectorStoreEntityVectoredFileEntity_Files_FilesId",
                        column: x => x.FilesId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VectorStoreEntityVectoredFileEntity_VectorStores_VectorStoreEntityId",
                        column: x => x.VectorStoreEntityId,
                        principalTable: "VectorStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Documents = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThreadId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssistantId = table.Column<string>(type: "varchar(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Assistants_AssistantId",
                        column: x => x.AssistantId,
                        principalTable: "Assistants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenUsage = table.Column<int>(type: "int", nullable: false),
                    Annotations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssistantMessageId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConversationId = table.Column<string>(type: "varchar(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assistants_InModelId",
                table: "Assistants",
                column: "InModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Assistants_StoreId",
                table: "Assistants",
                column: "StoreId",
                unique: true,
                filter: "[StoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_AssistantId",
                table: "Conversations",
                column: "AssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ThreadId",
                table: "Conversations",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_InModelId",
                table: "Files",
                column: "InModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AssistantMessageId",
                table: "Messages",
                column: "AssistantMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_VectorStoreEntityVectoredFileEntity_VectorStoreEntityId",
                table: "VectorStoreEntityVectoredFileEntity",
                column: "VectorStoreEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_VectorStores_InModelId",
                table: "VectorStores",
                column: "InModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "VectorStoreEntityVectoredFileEntity");

            migrationBuilder.DropTable(
                name: "YourEntity");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Assistants");

            migrationBuilder.DropTable(
                name: "VectorStores");
        }
    }
}
