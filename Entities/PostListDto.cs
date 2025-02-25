namespace BulletinBoard.Entities
{
    /// <summary>
    /// 用於GET格式化輸出
    /// </summary>
    public class PostListDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Time { get; set; }

        public int CommentId { get; set; }
        public List<FileDto>? Files { get; set; }
        public List<CommmentDto>? Comments { get; set; }
    }
    public class AccountListDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Password {  get; set; }
        public List<CommmentDto>? Comments { get; set; }

    }
    public class AccountList
    {
        public string? Name { get; set; }

    }
}
