using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Entities
{
    /// <summary>
    /// Comment資料表
    /// </summary>
    public class Comment
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public int? PostId { get; set; }
    }

    /// <summary>
    /// 上傳留言時的表格
    /// </summary>
    public class CommmentDto
    {
        public int? PostId { get; set; }
        public int? Id { get; set; }
        [Required]
        [StringLength(20)]
        public string? Name { get; set; }
        [Required]
        [StringLength(100)]
        public string? Content { get; set; }
        public string? Time {  get; set; }
    };
}
