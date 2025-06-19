using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetPostgresAuth.Data;
using AspNetPostgresAuth.Models;

namespace AspNetPostgresAuth.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(posts);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Title,Content")] Post post)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // 현재 사용자 정보를 PostgreSQL 세션에 설정
                    await _context.Database.ExecuteSqlRawAsync(
                        "SET application_name = 'AspNetPostgresAuth-User:{0}-CreatePost'", 
                        user.UserName);

                    post.AuthorId = user.Id;
                    post.CreatedAt = DateTime.UtcNow;
                    post.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Add(post);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "포스트가 성공적으로 작성되었습니다.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (post.AuthorId != user.Id)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(post);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // 기존 포스트를 데이터베이스에서 가져와서 권한 검사
            var existingPost = await _context.Posts.FindAsync(id);
            if (existingPost == null)
            {
                return NotFound();
            }

            // 작성자 권한 검사
            if (existingPost.AuthorId != user.Id)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 현재 사용자 정보를 PostgreSQL 세션에 설정
                    await _context.Database.ExecuteSqlRawAsync(
                        "SET application_name = 'AspNetPostgresAuth-User:{0}-EditPost'", 
                        user.UserName);

                    // 수정 가능한 필드만 업데이트
                    existingPost.Title = post.Title;
                    existingPost.Content = post.Content;
                    existingPost.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "포스트가 성공적으로 수정되었습니다.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // ModelState가 유효하지 않을 때도 기존 포스트 정보 유지
            post.AuthorId = existingPost.AuthorId;
            post.CreatedAt = existingPost.CreatedAt;
            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (post == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (post.AuthorId != user.Id)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && post.AuthorId == user.Id)
                {
                    // 현재 사용자 정보를 PostgreSQL 세션에 설정
                    await _context.Database.ExecuteSqlRawAsync(
                        "SET application_name = 'AspNetPostgresAuth-User:{0}-DeletePost'", 
                        user.UserName);

                    _context.Posts.Remove(post);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "포스트가 성공적으로 삭제되었습니다.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
} 