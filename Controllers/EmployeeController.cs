using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;   //--//
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;   //--//
using System.IdentityModel.Tokens.Jwt;   //--//
using System.Security.Claims;            //--//
using System.Text;                      //--//
using WorkTrackPro.API.Data;     //--//
using WorkTrackPro.API.Models;  //-//
namespace WorkTrackPro.API.Controller  
{
    [Route("api/[controller]")]     //--//
    [ApiController]                 //--//
    public class EmployeeController : ControllerBase    //--//
    {
        private readonly AppDbContext _context;        //--//
        private readonly IConfiguration _config;         //--//

        public EmployeeController(AppDbContext context, IConfiguration config)   //--//
        {
            _context = context;                    //--//
            _config = config;                     //--//
        }




        // 🟢 GET API                  //--//
        [Authorize]                  //--//
        [HttpGet]
        public IActionResult GetEmployees()
        {
            var data = _context.Employees.ToList();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public IActionResult GetEmployeeById(int id)
        {
            var data = _context.Employees.Find(id);

            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }


        // 🔵 POST API                    //--//
        [HttpPost]
        public IActionResult AddEmployee(Employee emp)
        {
            var exists = _context.Employees.Any(e => e.Email == emp.Email);

            if (exists)
            {
                return BadRequest("Email already exists");
            }

            _context.Employees.Add(emp);
            emp.JoinDate = DateTime.Now;
            _context.SaveChanges();

            return Ok(emp);
        }

        [HttpPost("send-otp")]
        public IActionResult SendOtp(
    ForgotPasswordDto dto)
        {
            var user = _context.Employees
                .FirstOrDefault(x => x.Email == dto.Email);

            if (user == null)
            {
                return NotFound("User Not Found");
            }

            Random random = new Random();

            string otp =
                random.Next(100000, 999999).ToString();

            user.Otp = otp;

            user.OtpExpiry =
                DateTime.Now.AddMinutes(5);

            _context.SaveChanges();

            Console.WriteLine($"OTP is: {otp}");

            return Ok("OTP Sent");
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp(
    VerifyOtpDto dto)
        {
            var user = _context.Employees
                .FirstOrDefault(x => x.Email == dto.Email);

            if (user == null)
            {
                return NotFound("User Not Found");
            }

            if (user.Otp != dto.Otp)
            {
                return BadRequest("Invalid OTP");
            }

            if (user.OtpExpiry < DateTime.Now)
            {
                return BadRequest("OTP Expired");
            }

            return Ok("OTP Verified");
        }

        [HttpPut("reset-password")]
        public IActionResult ResetPassword(
    ResetPasswordDto dto)
        {
            var user = _context.Employees
                .FirstOrDefault(x => x.Email == dto.Email);

            if (user == null)
            {
                return NotFound("User Not Found");
            }

            user.Password = dto.NewPassword;

            user.Otp = null;

            user.OtpExpiry = null;

            _context.SaveChanges();

            return Ok("Password Reset Successful");
        }

        // 🟡 PUT API                                 //--//
        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, Employee emp)
        {
            var data = _context.Employees.Find(id);

            if (data == null)
            {
                return NotFound("Employee not found");
            }

            data.Name = emp.Name;
            data.Email = emp.Email;
            data.Password = emp.Password;
            data.Role = emp.Role;
            data.Status = emp.Status;
            data.Phone = emp.Phone;
            data.Username = emp.Username;

            _context.SaveChanges();

            return Ok("Updated Successfully");
        }

        [HttpPut("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordDto dto)
        {
            var user = _context.Employees
                .FirstOrDefault(x => x.Email == dto.Email);

            if (user == null)
            {
                return NotFound("User Not Found");
            }

            user.Password = dto.NewPassword;

            _context.SaveChanges();

            return Ok("Password Updated");
        }




        // 🔴 DELETE API                             //--//
        [HttpDelete("{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            var data = _context.Employees.Find(id);

            if (data == null)
            {
                return NotFound("Employee not found");
            }

            _context.Employees.Remove(data);
            _context.SaveChanges();

            return Ok("Deleted Successfully");
        }

        // 🔴 LOGIN API                         //--//
        [HttpPost("login")]
        public IActionResult Login(LoginDto login)
        {
            var user = _context.Employees
                .FirstOrDefault(e => e.Email == login.Email && e.Password == login.Password);

            if (user == null)
            {
                return BadRequest("Invalid email or password");
            }

            var claims = new[]       //--//
    {
        new Claim(ClaimTypes.Name, user.Email!),
        new Claim(ClaimTypes.Role, user.Role!)
    };

            var keyString = _config["Jwt:Key"] ?? "ThisIsMySuperSecretKey@1234567890SecureKey";

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(keyString)
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                employeeId = user.Id,
                name = user.Name,

                profileImage = user.ProfileImage
            });
        }
    }
}


