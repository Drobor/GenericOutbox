﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Test.Migrations
{
    [DbContext(typeof(TestDbContext))]
    [Migration("20230415004516_nullability2")]
    partial class nullability2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("GenericOutbox.DataAccess.Entities.OutboxEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Exception")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<Guid?>("HandlerLock")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Payload")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("Response")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<int>("RetriesCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("RetryTimeout")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("HandlerLock");

                    b.HasIndex("ParentId")
                        .IsUnique();

                    b.HasIndex("Version");

                    b.ToTable("OutboxEntities");
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
