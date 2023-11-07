﻿// <auto-generated />
using System;
using GenericOutbox.ManagementUi.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GenericOutbox.ManagementUi.App.Migrations
{
    [DbContext(typeof(ManagementUiDbContext))]
    partial class ManagementUiDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("GenericOutbox.DataAccess.Entities.OutboxDataEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ScopeId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ScopeId");

                    b.ToTable("OutboxDataEntity");
                });

            modelBuilder.Entity("GenericOutbox.DataAccess.Entities.OutboxEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("HandlerLock")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdatedUtc")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("Lock")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParentId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Payload")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<int>("RetriesCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("RetryTimeoutUtc")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ScopeId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("HandlerLock");

                    b.HasIndex("Lock");

                    b.HasIndex("ParentId")
                        .IsUnique();

                    b.HasIndex("ScopeId");

                    b.HasIndex("Version");

                    b.ToTable("OutboxEntity");
                });

            modelBuilder.Entity("GenericOutbox.DataAccess.Entities.OutboxEntity", b =>
                {
                    b.HasOne("GenericOutbox.DataAccess.Entities.OutboxEntity", "Parent")
                        .WithOne()
                        .HasForeignKey("GenericOutbox.DataAccess.Entities.OutboxEntity", "ParentId");

                    b.Navigation("Parent");
                });
#pragma warning restore 612, 618
        }
    }
}
