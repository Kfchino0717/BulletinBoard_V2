using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BulletinBoard
{
    public partial class BulletinBoardContext : DbContext
    {
        public BulletinBoardContext()
        {
        }

        public BulletinBoardContext(DbContextOptions<BulletinBoardContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Account { get; set; } = null!;
        public virtual DbSet<Comment> Comment { get; set; } = null!;
        public virtual DbSet<Files> Files { get; set; } = null!;
        public virtual DbSet<Post> Post { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");

                entity.HasComment("登入用帳號密碼");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(10)
                    .HasColumnName("name")
                    .HasComment("名字");

                entity.Property(e => e.Password)
                    .HasMaxLength(20)
                    .HasColumnName("password")
                    .HasComment("密碼");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("comment");

                entity.HasComment("留言");

                entity.HasIndex(e => e.PostId, "post_id_set_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasComment("流水編號");

                entity.Property(e => e.Content)
                    .HasMaxLength(100)
                    .HasColumnName("content")
                    .HasComment("留言內容");

                entity.Property(e => e.Name)
                    .HasMaxLength(45)
                    .HasColumnName("name")
                    .HasComment("留言者姓名");

                entity.Property(e => e.PostId)
                    .HasColumnName("post_id")
                    .HasComment("留言使用者編號");

                entity.Property(e => e.Time)
                    .HasMaxLength(45)
                    .HasColumnName("time");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Comment)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("post_id2_set");
            });

            modelBuilder.Entity<Files>(entity =>
            {
                entity.ToTable("files");

                entity.HasIndex(e => e.PostId, "post_id_set_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasComment("文件編號");

                entity.Property(e => e.FileName)
                    .HasMaxLength(45)
                    .HasColumnName("file_name")
                    .HasComment("文件名字");

                entity.Property(e => e.FilePath)
                    .HasMaxLength(45)
                    .HasColumnName("file_path")
                    .HasComment("文件路徑");

                entity.Property(e => e.PostId)
                    .HasColumnName("post_id")
                    .HasComment("文件使用者編號");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("id_fk");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("post");

                entity.HasComment("留言板");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasComment("流水編號");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content")
                    .HasComment("內容");

                entity.Property(e => e.Time)
                    .HasMaxLength(45)
                    .HasColumnName("time");

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasColumnName("title")
                    .HasComment("標題");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
