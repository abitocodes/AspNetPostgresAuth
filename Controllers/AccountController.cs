using AspNetPostgresAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AspNetPostgresAuth.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            return View(model);

            // IdentityUser
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if(result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string userName, string password, bool rememberMe = false)
        {
            if(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "아이디와 비밀번호를 입력해주세요.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(userName, password, rememberMe, lockoutOnFailure: false);

            if(result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "로그인 실패: 아이디 또는 비밀번호를 확인하세요.");
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ------------------------------
        // [1] GET: /Account/EditProfile
        // 로그인된 사용자의 현재 이메일을 뷰에 뿌려줍니다.
        // ------------------------------
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new EditProfileViewModel
            {
                NewEmail = user.Email 
            };

            return View(model);
        }

        // ------------------------------
        // [2] POST: /Account/EditProfile
        // 사용자가 입력한 NewEmail로 실제 IdentityUser.Email을 업데이트
        // ------------------------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // 유효성 검사 실패시 폼을 다시 보여줌
                return View(model);
            }

            // 현재 로그인된 IdentityUser 가져오기
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // 만약 User가 null이면 재로그인 유도
                return RedirectToAction("Login", "Account");
            }

            // 새 이메일이 기존과 동일하면 별도 처리 없이 리다이렉트
            if (model.NewEmail == user.Email)
            {
                ViewBag.StatusMessage = "이메일이 이전 값과 동일합니다.";
                // 폼에 현재 값 그대로 띄우기 위해 CurrentEmail만 유지
                // model.CurrentEmail = user.Email;
                return View(model);
            }

            // 1) IdentityUser.Email을 변경
            // IdentityUser 클래스에서는 SetEmailAsync를 사용하면 NormalizedEmail도 내부적으로 처리해줍니다.
            var setEmailResult = await _userManager.SetEmailAsync(user, model.NewEmail);
            if (!setEmailResult.Succeeded)
            {
                // 오류가 발생한 경우 ModelState에 에러를 추가
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // CurrentEmail을 최신(user.Email)으로 수정 후 폼 재표시
                // model.CurrentEmail = user.Email;
                return View(model);
            }

            // 2) (선택 사항) UserName을 이메일로 사용 주잉었다면, 다음과 같이 UserName도 변경할 수 있습니다.
            //    // await _userManager.SetUserNameAsync(user, model.NewEmail);

            // 3) 성공적으로 이메일이 변경된 경우 세션(Claims)을 갱신하려면 아래 두 줄을 호출
            await _signInManager.RefreshSignInAsync(user);

            // 4) 성공 메시지를 ViewBag 등에 담아 뷰로 전달하거나, 다른 페이지로 리다이렉트
            ViewBag.StatusMessage = "이메일이 성공적으로 변경되었습니다.";
            // CurrentEmail 값을 새로 이메일로 갱신해서 뷰에 띄움.
            // model.CurrentEmail = model.NewEmail;

            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}