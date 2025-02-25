using System.ComponentModel.DataAnnotations;

/// <summary>
/// File資料表
/// Post進行關聯用
/// </summary>
public class File
{
    public int? Id { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public int PostId { get; set; }
    public Post? Postpost { get; set; }
}

/// <summary>
/// GET時進行資料排序時用
/// </summary>
public class FileDto
{
    public int? Id { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
}

/// <summary>
/// Post資料表
/// 進行資料和留言的關聯
/// </summary>
public class Post
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public ICollection<File>? Files { get; set; } 
    public ICollection<Comment>? Comments { get; set; }
}

/// <summary>
/// 上傳貼文時的表格
/// </summary>
public class PostDto
{
    [Required]
    [StringLength(20)]
    public string? Title { get; set; }

    [Required]
    [StringLength(500)]
    public string? Content { get; set; }
    
    public List<IFormFile>? files { get; set; }

};

public class AccountDto
{
    [Required]
    [StringLength(5)]
    public string? Name { get; set; }

    [Required]
    [StringLength(10)]
    public string?  Password{ get; set; }
}

public class Account
{
    public string? Name { get; set; }
    public string? Password { get; set; }
}

/// <summary>
/// Comment資料表
/// Post進行關聯用
/// </summary>
public class Comment
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Content { get; set; }
    public DateTime Time { get; set; }
    public int PostId { get; set; }
}
