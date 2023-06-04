using ChatServer.Data;
using ChatServer.DTO;
using ChatServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatServer.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ChatServerContext _context;
        private readonly JWTSettings _options;

        public UsersController(IOptions<JWTSettings> options, ChatServerContext context)
        {
            _options = options.Value;
            _context = context;
        }


        /// <summary>
        /// Return token from login.
        /// </summary>
        /// <response code="400">
        /// Incorrect login:
        /// 
        ///     {
        ///         "code": 3003,
        ///         "message": "Invalid login"
        ///     }
        /// 
        /// Incorrect password:
        /// 
        ///     {
        ///         "code": 3003,
        ///         "message": "Invalid password"
        ///     }
        /// </response>
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserReturnWithTokenDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
        public async Task<IActionResult> Login(LoginInputDTO user_request)
        {
            var user = await _context.User.FirstOrDefaultAsync(x => x.email == user_request.email);
            if (user == null)
            {
                return BadRequest(new ErrorDTO(3003, "Invalid login"));
            }
            if (user.password != user_request.password)
            {

                return BadRequest(new ErrorDTO(3003, "Invalid password"));
            }

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Role, "User")
            };

            claims.Add(new Claim("Email", user.email));
            claims.Add(new Claim("Id", user.id_user.ToString()));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

            UserReturnWithTokenDTO userReturnDTO = new UserReturnWithTokenDTO();
            userReturnDTO.token = new JwtSecurityTokenHandler().WriteToken(jwt);
            userReturnDTO.id_user = user.id_user;
            userReturnDTO.nickname = user.nickname;
            userReturnDTO.email = user.email;

            return Ok(userReturnDTO);
        }


        /// <summary>
        /// Create user and return token.
        /// </summary>
        /// <response code="400">
        /// User already exists:
        /// 
        ///     {
        ///         "code": 3033,
        ///         "message": "Unable to register user. User already exists."
        ///     }
        /// </response>
        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserReturnWithTokenDTO))]
        public async Task<IActionResult> Register(UserRegisterDTO user_request)
        {
            var user = await _context.User.FirstOrDefaultAsync(x => x.email == user_request.email);
            if (user != null)
            {
                return BadRequest(new ErrorDTO(3033, "Unable to register user. User already exists."));
            }

            var tempuser = new User
            {
                nickname = user_request.nickname,
                email = user_request.email,
                password = user_request.password
            };

            _context.User.Add(tempuser);
            await _context.SaveChangesAsync();
            return await Login(new LoginInputDTO() { email = tempuser.email, password = tempuser.password });
        }

        /// <summary>
        /// Edit user.
        /// </summary>
        /// <response code="400">
        /// Incorrect password:
        /// 
        ///     {
        ///         "code": 3003,
        ///         "message": "Invalid password"
        ///     }
        /// Incorrect password:
        /// 
        ///     {
        ///         "code": 3033,
        ///         "message": "Unable to change email. User with this email already exists."
        ///     }
        /// </response>
        [HttpPost("EditUser")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserReturnDTO))]
        public async Task<IActionResult> EditUser(UserEditDTO user_edit)
        {
            int id_user = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
            var user = await _context.User.FirstOrDefaultAsync(x => x.id_user == id_user);
            if(user.password != user_edit.password)
            {
                return BadRequest(new ErrorDTO(3003, "Invalid password"));
            }

            if(!user_edit.new_password.IsNullOrEmpty()) { 
                user.password = user_edit.new_password;
            }

            if (user_edit.email != user.email)
            {
                if (!user_edit.email.IsNullOrEmpty())
                {
                    if (await _context.User.FirstOrDefaultAsync(x => x.email == user_edit.email) != null)
                    {
                        return BadRequest(new ErrorDTO(3033, "Unable to change email. User with this email already exists."));
                    }
                    user.email = user_edit.email;
                }
            }

            if(user_edit.nickname != user.nickname)
            {
                if (!user_edit.nickname.IsNullOrEmpty())
                {
                    user.nickname = user_edit.nickname;
                }
            }

            user = _context.User.Update(user).Entity;
            await _context.SaveChangesAsync();

            UserReturnDTO userReturnDTO = new UserReturnDTO();
            userReturnDTO.id_user = user.id_user;
            userReturnDTO.nickname = user.nickname;
            userReturnDTO.email = user.email;

            return Ok(userReturnDTO);
        }

        /// <summary>
        /// Find User.
        /// </summary>
        /// <response code="400">
        /// Incorrect login:
        /// 
        ///     {
        ///         "code": 3003,
        ///         "message": "Invalid login"
        ///     }
        /// 
        /// Incorrect password:
        /// 
        ///     {
        ///         "code": 3003,
        ///         "message": "Invalid password"
        ///     }
        /// </response>
        [Authorize]
        [HttpGet("FindUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserReturnDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
        public async Task<IActionResult> FindUser(string user_data_to_find)
        {
            List<User> users_by_nickname = await _context.User.Where(x => x.nickname.Contains(user_data_to_find)).ToListAsync();
            List<User> users_by_email = await _context.User.Where(x => x.email.Contains(user_data_to_find)).ToListAsync();
            List<User> users = users_by_email.Union(users_by_nickname).ToList();
            if (users.Count() == 0)
            {
                return BadRequest();
            }

            List<UserReturnDTO> result = new List<UserReturnDTO>();

            foreach (var user in users)
            {
                UserReturnDTO userReturn = new();
                userReturn.id_user = user.id_user;
                userReturn.nickname = user.nickname;
                userReturn.email = user.email;
                result.Add(userReturn);
            }

            return Ok(result);
        }

        [HttpGet("GetAllUsers")]
        public async Task<List<UserReturnDTO>> GetAllUsers()
        {
            List<User> users = await _context.User.ToListAsync();

            List<UserReturnDTO> result = new();

            foreach (var user in users)
            {
                UserReturnDTO userReturn = new();

                userReturn.id_user = user.id_user;
                userReturn.nickname = user.nickname;
                userReturn.email = user.email;
                result.Add(userReturn);
            }

            return result;
        }

        [HttpGet("GetUserById")]
        public async Task<UserReturnDTO> GetUserById(int id)
        {
            User user = await _context.User.FirstOrDefaultAsync(c => c.id_user == id);

            UserReturnDTO userReturn = new();

            userReturn.id_user = user.id_user;
            userReturn.nickname = user.nickname;
            userReturn.email = user.email;

            return userReturn;
        }
    }
}
