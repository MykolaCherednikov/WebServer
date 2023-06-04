using ChatServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data;

public partial class ChatServerContext : DbContext
{
    public ChatServerContext()
    {

    }

    public ChatServerContext(DbContextOptions<ChatServerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat> Chat { get; set; }

    public virtual DbSet<Message> Message { get; set; }

    public virtual DbSet<TypeChat> TypeChat { get; set; }

    public virtual DbSet<User> User { get; set; }

    public virtual DbSet<UserToChat> UserToChat { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=109.87.235.191\\SQLSERVER; Initial Catalog=messenger; Persist Security Info=True; User ID=Simixman; Password=123; Encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.id_chat);

            entity.ToTable("Chat");

            entity.HasIndex(e => e.link, "link_UNIQUE").IsUnique();

            entity.Property(e => e.id_chat).HasColumnName("id_chat");
            entity.Property(e => e.link)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("link");
            entity.Property(e => e.name_chat)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("name_chat");
            entity.Property(e => e.rk_type_chat).HasColumnName("rk_type_chat");

            entity.HasOne(d => d.RkTypeChatNavigation).WithMany(p => p.Chats)
                .HasForeignKey(d => d.rk_type_chat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Type_Chat");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.id_message);

            entity.ToTable("Message");

            entity.Property(e => e.id_message).HasColumnName("id_message");
            entity.Property(e => e.data_time)
                .HasColumnType("datetime")
                .HasColumnName("data_time");
            entity.Property(e => e.rk_chat).HasColumnName("rk_chat");
            entity.Property(e => e.rk_user).HasColumnName("rk_user");
            entity.Property(e => e.text_message)
                .HasColumnType("text")
                .HasColumnName("text_message");

            entity.HasOne(d => d.RkChatNavigation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.rk_chat)
                .HasConstraintName("FK_Message_Chat");

            entity.HasOne(d => d.RkUserNavigation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.rk_user)
                .HasConstraintName("FK_Message_User");
        });

        modelBuilder.Entity<TypeChat>(entity =>
        {
            entity.HasKey(e => e.id_chat_type);

            entity.ToTable("TypeChat");

            entity.Property(e => e.id_chat_type).HasColumnName("id_chat_type");
            entity.Property(e => e.name_chat_type)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("name_chat_type");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.id_user);
            
            entity.ToTable("User");

            entity.HasIndex(e => e.email, "UNIQUE_email").IsUnique();

            entity.Property(e => e.id_user).HasColumnName("id_user");
            entity.Property(e => e.email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.nickname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nickname");
            entity.Property(e => e.password)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("password");
        });

        modelBuilder.Entity<UserToChat>(entity =>
        {
            entity.HasKey(e => e.id_connection);

            entity.ToTable("UserToChat");

            entity.Property(e => e.id_connection).HasColumnName("id_connection");
            entity.Property(e => e.rk_id_chat).HasColumnName("rk_id_chat");
            entity.Property(e => e.rk_id_user).HasColumnName("rk_id_user");

            entity.HasOne(d => d.RkIdChatNavigation).WithMany(p => p.Chat_UserToChats)
                .HasForeignKey(d => d.rk_id_chat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserToChat_Chat");

            entity.HasOne(d => d.RkIdUserNavigation).WithMany(p => p.User_UserToChats)
                .HasForeignKey(d => d.rk_id_user)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserToChat_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}



