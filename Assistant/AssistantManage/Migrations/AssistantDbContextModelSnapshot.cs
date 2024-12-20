﻿// <auto-generated />
using System;
using AssistantManage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AssistantManage.Migrations
{
    [DbContext(typeof(AssistantDbContext))]
    partial class AssistantDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AssistantManage.Data.Models.AssistantEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .IsUnicode(false)
                        .HasColumnType("varchar(36)");

                    b.Property<string>("Base64Avatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("HasCodeInterpreter")
                        .HasColumnType("bit");

                    b.Property<bool>("HasFileSearch")
                        .HasColumnType("bit");

                    b.Property<string>("InModelId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Instructions")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StoreId")
                        .HasColumnType("varchar(36)");

                    b.Property<float>("Temperature")
                        .HasColumnType("real");

                    b.Property<int>("Tools")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("InModelId");

                    b.HasIndex("StoreId")
                        .IsUnique()
                        .HasFilter("[StoreId] IS NOT NULL");

                    b.ToTable("Assistants");
                });

            modelBuilder.Entity("AssistantManage.Data.Models.ConversationEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .IsUnicode(false)
                        .HasColumnType("varchar(36)");

                    b.Property<string>("AssistantId")
                        .IsRequired()
                        .HasColumnType("varchar(36)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2");

                    b.Property<string>("Documents")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ThreadId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AssistantId");

                    b.HasIndex("ThreadId");

                    b.ToTable("Conversations");
                });

            modelBuilder.Entity("AssistantManage.Data.Models.MessageEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .IsUnicode(false)
                        .HasColumnType("varchar(36)");

                    b.Property<string>("Annotations")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AssistantMessageId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConversationId")
                        .IsRequired()
                        .HasColumnType("varchar(36)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2");

                    b.Property<string>("Sender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TokenUsage")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AssistantMessageId");

                    b.HasIndex("ConversationId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("AssistantManage.Data.Models.VectorStoreEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .IsUnicode(false)
                        .HasColumnType("varchar(36)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2");

                    b.Property<string>("InModelId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("InModelId");

                    b.ToTable("VectorStores");
                });

            modelBuilder.Entity("AssistantManage.Data.Models.VectoredFileEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .IsUnicode(false)
                        .HasColumnType("varchar(36)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2");

                    b.Property<string>("ExternalSource")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InModelId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Purpose")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SizeInBytes")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("InModelId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("AssistantManage.Data.YourEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("YourEntity");
                });

            modelBuilder.Entity("VectorStoreEntityVectoredFileEntity", b =>
                {
                    b.Property<string>("FilesId")
                        .HasColumnType("varchar(36)");

                    b.Property<string>("VectorStoreEntityId")
                        .HasColumnType("varchar(36)");

                    b.HasKey("FilesId", "VectorStoreEntityId");

                    b.HasIndex("VectorStoreEntityId");

                    b.ToTable("VectorStoreEntityVectoredFileEntity");
                });

            modelBuilder.Entity("AssistantManage.Data.Models.AssistantEntity", b =>
                {
                    b.HasOne("AssistantManage.Data.Models.VectorStoreEntity", "Store")
                        .WithOne()
                        .HasForeignKey("AssistantManage.Data.Models.AssistantEntity", "StoreId");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("AssistantManage.Data.Models.ConversationEntity", b =>
                {
                    b.HasOne("AssistantManage.Data.Models.AssistantEntity", "Assistant")
                        .WithMany()
                        .HasForeignKey("AssistantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Assistant");
                });

            modelBuilder.Entity("AssistantManage.Data.Models.MessageEntity", b =>
                {
                    b.HasOne("AssistantManage.Data.Models.ConversationEntity", null)
                        .WithMany("Messages")
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VectorStoreEntityVectoredFileEntity", b =>
                {
                    b.HasOne("AssistantManage.Data.Models.VectoredFileEntity", null)
                        .WithMany()
                        .HasForeignKey("FilesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AssistantManage.Data.Models.VectorStoreEntity", null)
                        .WithMany()
                        .HasForeignKey("VectorStoreEntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AssistantManage.Data.Models.ConversationEntity", b =>
                {
                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
