using BulletinBoard;
using BulletinBoard.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("api/[controller]/[action]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly BulletinBoardContext _context;
    private readonly string _folder;
    private readonly static Dictionary<string, string> _contentTypes = new Dictionary<string, string>//可下載的文件格式
    {
        {".png", "image/png"},
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".gif", "image/gif"},
        {".docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        {".pdf","application/pdf" },
        {".txt","application/txt"}
    };

    /// <summary>
    /// 判斷是否存在
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool PostExists(int id)
    {
        return _context.Post.Any(e => e.Id == id);
    }

    public PostController(BulletinBoardContext context)
    {
        _context = context;
        _folder = "UploadedFiles";
        if (!Directory.Exists(_folder))
        {
            Directory.CreateDirectory(_folder);
        }
    }
    /// <summary>
    /// 帳號密碼列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountList>>> GetAccount()
    { var result = await _context.Account
                     .Select(g => new AccountList
                     {
                         Name = g.Name,
                     }).ToListAsync();
        return Ok(result);
    }

    /// <summary>
    /// 顯示指定留言
    /// </summary>
    /// <returns></returns>
    [HttpGet("{name}")]
    public async Task<ActionResult<IEnumerable<AccountListDto>>> GetAccountPostComment([FromRoute]string name)
    {
        var account = await _context.Account.FirstOrDefaultAsync(a => a.Name == name);
        if(account == null) return NotFound();
        var comments = await _context.Account
            .Where(f => f.Name == name)
            .GroupJoin(
                _context.Comment,
                account => account.Name,
                comments => comments.Name,
                (account, comments) => new { Account = account, Comment = comments }
            ).SelectMany(
                x => x.Comment.DefaultIfEmpty(),
                (Account, comment) => new { Account.Account, Comment = comment }
            ).ToListAsync();
        var result = comments
            .GroupBy(ac => ac.Account.Name)
            .Select(g => new AccountListDto
            {
                Name = g.First().Account.Name,
                Comments = g.Where(x => x.Comment != null)
                    .Select(c => new CommmentDto
                    {
                        Id = c.Comment?.Id,
                        PostId = c.Comment?.PostId,
                        Name = c.Comment?.Name,
                        Content = c.Comment?.Content,
                        Time = c.Comment?.Time,
                    })
                .ToList()
            }).ToList();
        return Ok(result);
    }

    /// <summary>
    /// 查表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostListDto>>> GetPost()
    {
        var postsWithFilesAndComments = await _context.Post
            .GroupJoin(
                _context.Files,
                post => post.Id,
                file => file.PostId,
                (post, files) => new { Post = post, Files = files }
            )
            .SelectMany(
                x => x.Files.DefaultIfEmpty(),
                (postFiles, file) => new { postFiles.Post, File = file }
            )
            .GroupJoin(
                _context.Comment,
                postFile => postFile.Post.Id,
                comment => comment.PostId,
                (postFile, comments) => new { postFile.Post, postFile.File, Comments = comments }
            )
            .SelectMany(
                x => x.Comments.DefaultIfEmpty(),
                (postFilesComments, comment) => new { postFilesComments.Post, postFilesComments.File, Comment = comment }
            )
            .ToListAsync();

        //pfc就是post+file+comment的結合體
        var result = postsWithFilesAndComments
            .GroupBy(pfc => pfc.Post.Id)
            .Select(g => new PostListDto
            {
                Id = g.First().Post.Id,
                Title = g.First().Post.Title,
                Content = g.First().Post.Content,
                Time = g.First().Post.Time,
                Files = g.Where(x => x.File != null)
                    .Select(f => new FileDto
                    {
                        Id = f.File?.Id,
                        FileName = f.File?.FileName,
                        FilePath = f.File?.FilePath,
                    })
                    .ToList(),
                Comments = g.Where(x => x.Comment != null)
                    .Select(c => new CommmentDto
                    {
                        Id = c.Comment?.Id,
                        Name = c.Comment?.Name,
                        Content = c.Comment?.Content,
                        Time = c.Comment?.Time,
                    })
                .ToList()
            }).ToList();

        return Ok(result);
    }



    /// <summary>
    /// 確認帳號資料正確性
    /// </summary>
    /// <returns></returns>
    [HttpGet("{name}/{password}")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> CheckAccount([FromRoute] string name, [FromRoute] string password)
    {
        var account = await _context.Account.FirstOrDefaultAsync(a => a.Name == name);
        if (account == null)
            return NotFound("未找到該筆資料");
        if (account.Password != password)
            return BadRequest("密碼錯誤");
        return Ok(account);
    }

    /// <summary>
    /// 尋找指定ID資料
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<PostListDto>> GetPost([FromRoute] int id)
    {
        var post = await _context.Post.FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
            return NotFound();

        var files = await _context.Files
            .Where(f => f.PostId == id)
            .Select(f => new FileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                FilePath = f.FilePath
            })
            .ToListAsync();

        var comments = await _context.Comment
            .Where(f => f.PostId == id)
            .Select(f => new CommmentDto
            {
                Id = f.Id,
                Name = f.Name,
                Content = f.Content,
                Time = f.Time,
            })
            .ToListAsync();

        var result = new PostListDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Time = post.Time,
            Files = files,
            Comments = comments,
        };

        return Ok(result);
    }

    /// <summary>
    /// 尋找指定主題
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    [HttpGet("{title}")]
    public async Task<ActionResult<PostListDto>> GetPostTitle([FromRoute] string title)
    {
        var post = await _context.Post.FirstOrDefaultAsync(p => p.Title == title);

        if (post == null)
            return NotFound();

        var files = await _context.Files
            .Where(f => f.PostId == post.Id)
            .Select(f => new FileDto
            {
                FileName = f.FileName,
                FilePath = f.FilePath
            })
            .ToListAsync();

        var comments = await _context.Comment
            .Where(f => f.PostId == post.Id)
            .Select(f => new CommmentDto
            {
                Id = f.Id,
                Name = f.Name,
                Content = f.Content,
                Time = f.Time,
            })
            .ToListAsync();

        var result = new PostListDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Time = post.Time,
            Files = files,
            Comments = comments,
        };

        return Ok(result);
    }

    /// <summary>
    /// 模糊搜尋
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{content}")]
    public async Task<ActionResult<IEnumerable<PostListDto>>> GetPostContent([FromRoute] string content)
    {
        var postsWithFilesAndComments = await _context.Post
            .GroupJoin(
                _context.Files,
                post => post.Id,
                file => file.PostId,
                (post, files) => new { Post = post, Files = files }
            )
            .SelectMany(
                x => x.Files.DefaultIfEmpty(),
                (postFiles, file) => new { postFiles.Post, File = file }
            )
            .GroupJoin(
                _context.Comment,
                postFile => postFile.Post.Id,
                comment => comment.PostId,
                (postFile, comments) => new { postFile.Post, postFile.File, Comments = comments }
            )
            .SelectMany(
                x => x.Comments.DefaultIfEmpty(),
                (postFilesComments, comment) => new { postFilesComments.Post, postFilesComments.File, Comment = comment }
            )
            .ToListAsync();

        //pfc就是post+file+comment的結合體
        var result = postsWithFilesAndComments
            .GroupBy(pfc => pfc.Post.Id)
            .Select(g => new PostListDto
            {
                Id = g.First().Post.Id,
                Title = g.First().Post.Title,
                Content = g.First().Post.Content,
                Time = g.First().Post.Time,
                Files = g.Where(x => x.File != null)
                    .Select(f => new FileDto
                    {
                        Id = f.File?.Id,
                        FileName = f.File?.FileName,
                        FilePath = f.File?.FilePath,
                    })
                    .ToList(),
                Comments = g.Where(x => x.Comment != null)
                    .Select(c => new CommmentDto
                    {
                        Id = c.Comment?.Id,
                        Name = c.Comment?.Name,
                        Content = c.Comment?.Content,
                        Time = c.Comment?.Time,
                    })
                .ToList()
            }).ToList();
        var trueResult = result.Where(a => a.Content.Contains(content));
        return Ok(trueResult);
    }

    /// <summary>
    /// 查詢單筆留言
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<PostListDto>> GetComment([FromRoute] int id)
    {
        var commentid = await _context.Comment.FirstOrDefaultAsync(p => p.Id == id);
        if (commentid == null) return NotFound();
        return Ok(commentid);
    }

    /// <summary>
    /// 創建帳號
    /// </summary>
    /// <param name="accountdate"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Post>> CreateAccount([FromForm] AccountDto accountdate)
    {
        var oldAccount = await _context.Account.FirstOrDefaultAsync(a => a.Name == accountdate.Name);
        if (oldAccount != null) return BadRequest();//重複註冊會有錯誤訊息
        var newAccount = new BulletinBoard.Account
        {
            Name = accountdate.Name,
            Password = accountdate.Password,
        };
        await _context.Account.AddAsync(newAccount);
        await _context.SaveChangesAsync();
        return Ok(newAccount);
    }

    /// <summary>
    /// 上傳資料v
    /// </summary>
    /// <param name="postdate"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost([FromForm] PostDto postdate)
    {
        var nowtime = DateTime.Now;
        var formattedDateTime = nowtime.ToString("yyyy/MM/dd HH:mm:ss");
        
        var newPost = new BulletinBoard.Post
        {
            Title = postdate.Title,
            Content = postdate.Content,
            Time = formattedDateTime,
        };
        await _context.Post.AddAsync(newPost);
        await _context.SaveChangesAsync();

        if (postdate.files != null && postdate.files.Count > 0)
        {
            foreach (var file in postdate.files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(_folder, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var newFile = new Files
                    {
                        PostId = newPost.Id,
                        FileName = file.FileName,
                        FilePath = filePath
                    };
                    await _context.Files.AddAsync(newFile);

                }

            }
        }
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPost), new { id = newPost.Id }, newPost);
    }

    /// <summary>
    /// 新增留言
    /// </summary>
    /// <param name="commmentDto"></param>
    /// <returns></returns>
    [HttpPost("{id}")]
    public async Task<ActionResult<Post>> CreateComment([FromRoute] int id, [FromForm] CommmentDto commmentDto)
    {
        DateTime nowtime = DateTime.Now;
        string formattedDateTime = nowtime.ToString("yyyy/MM/dd HH:mm:ss");
        var post = await _context.Post.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return NotFound();
        }
        if (commmentDto.Content != null && commmentDto.Name != null)
        {
            var newcomment = new BulletinBoard.Comment
            {
                PostId = post.Id,
                Name = commmentDto.Name,
                Content = commmentDto.Content,
                Time = formattedDateTime,
            };
            await _context.Comment.AddAsync(newcomment);
        }
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(id))
            {
                return NotFound();
            }
            else
            {
                //例外
                throw;
            }
        }
        return NoContent();
    }

    /// <summary>
    /// 更新留言
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateform"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromForm]CommmentDto updatecomment)
    {
        DateTime nowtime = DateTime.Now;
        string formattedDateTime = nowtime.ToString("yyyy/MM/dd HH:mm:ss");
        var comment = await _context.Comment.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }
        comment.Name = updatecomment.Name;
        comment.Content = updatecomment.Content;
        comment.Time = formattedDateTime;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(id))
            {
                return NotFound();
            }
            else
            {
                //例外
                throw;
            }
        }
        return NoContent();
    }

    /// <summary>
    /// 更新上傳檔案v
    /// </summary>
    /// <param name="id">編號</param>
    /// <param name="updateform">更新內容</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromForm] PostDto updateform)
    {
        DateTime nowtime = DateTime.Now;
        string formattedDateTime = nowtime.ToString("yyyy/MM/dd HH:mm:ss");
        var post = await _context.Post.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return NotFound();
        }
        post.Title = updateform.Title;
        post.Content = updateform.Content;
        post.Time = formattedDateTime;
        if (updateform.files != null && updateform.files.Count > 0)
        {
            foreach (var file in updateform.files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(_folder, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var newFile1 = new Files
                    {
                        PostId = post.Id,
                        FileName = file.FileName,
                        FilePath = filePath,
                    };
                    await _context.Files.AddAsync(newFile1);
                }
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(id))
            {
                return NotFound();
            }
            else
            {
                //例外
                throw;
            }
        }
        var answer = await _context.Comment.FirstOrDefaultAsync(c => c.PostId == id);
        return Ok(answer);
    }


    /// <summary>
    /// 取得文件v
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpGet("{fileName}")]
    public async Task<IActionResult> Download([FromRoute] string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return NotFound();
        }

        var path = Path.Combine(_folder, fileName);
        var memoryStream = new MemoryStream();
        using (var stream = new FileStream(path, FileMode.Open))
        {
            await stream.CopyToAsync(memoryStream);
        }
        memoryStream.Seek(0, SeekOrigin.Begin);
        return new FileStreamResult(memoryStream, _contentTypes[Path.GetExtension(path).ToLowerInvariant()]);
    }

    /// <summary>
    /// 由(file)ID進行下載資料
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> IDDownload([FromRoute] int id)
    {
        var fileid = await _context.Files.FirstOrDefaultAsync(p => p.Id == id);
        if (fileid == null)
            return NotFound();
        var fileName = fileid.FileName;
        if (fileName == null)
            return NotFound("找不到指定id的資料");
        var path = Path.Combine(_folder, fileName);
        var memoryStream = new MemoryStream();
        using (var stream = new FileStream(path, FileMode.Open))
        {
            await stream.CopyToAsync(memoryStream);
        }
        memoryStream.Seek(0, SeekOrigin.Begin);
        return new FileStreamResult(memoryStream, _contentTypes[Path.GetExtension(path).ToLowerInvariant()]);
    }

    /// <summary>
    /// 由ID刪除資料
    /// </summary>
    /// <param name="id"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost([FromRoute] int id)
    {
        var post = await _context.Post.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return NotFound("找不到指定id的資料");
        }
        _context.Post.Remove(post);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile([FromRoute] int id)
    {
        var file = await _context.Files.FirstOrDefaultAsync(p => p.Id == id);
        if (file == null)
        {
            return NotFound("找不到指定id的留言");
        }
        _context.Files.Remove(file);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// 刪除留言
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment([FromRoute] int id)
    {
        var comment = await _context.Comment.FirstOrDefaultAsync(p => p.Id == id);
        if (comment == null)
        {
            return NotFound("找不到指定id的檔案");
        }
        _context.Comment.Remove(comment);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

internal record struct NewStruct(object Item1, object Item2)
{
    public static implicit operator (object, object)(NewStruct value)
    {
        return (value.Item1, value.Item2);
    }

    public static implicit operator NewStruct((object, object) value)
    {
        return new NewStruct(value.Item1, value.Item2);
    }
}