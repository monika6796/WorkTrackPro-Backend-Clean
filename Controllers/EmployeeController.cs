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
            try
            {
                var exists = _context.Employees.Any(e => e.Email == emp.Email);

                if (exists)
                {
                    return BadRequest("Email already exists");
                }

                emp.JoinDate = DateTime.UtcNow;

                _context.Employees.Add(emp);
                _context.SaveChanges();

                return Ok(emp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
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
        [HttpPost("seed")]
        public IActionResult SeedEmployees()
        {
            if (_context.Employees.Any())
            {
                return BadRequest("Employees already exist.");
            }

            var employees = new List<Employee>
    {
        new Employee { Name="Monika Pandey", Email="monika@gmail.com", Password="Monika@123", Role="Admin", Status="Active", Phone="9876543200", Username="monika6796", JoinDate=DateTime.UtcNow },

        new Employee { Name="Rahul Sharma", Email="rahul@gmail.com", Password="Rahul@123", Role="Employee", Status="Active", Phone="9876543201", Username="rahul01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Priya Verma", Email="priya@gmail.com", Password="Priya@123", Role="Employee", Status="Active", Phone="9876543202", Username="priya01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Amit Singh", Email="amit@gmail.com", Password="Amit@123", Role="Employee", Status="Active", Phone="9876543203", Username="amit01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Neha Gupta", Email="neha@gmail.com", Password="Neha@123", Role="Employee", Status="Active", Phone="9876543204", Username="neha01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Rohit Meena", Email="rohit@gmail.com", Password="Rohit@123", Role="Employee", Status="Active", Phone="9876543205", Username="rohit01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Anjali Sharma", Email="anjali@gmail.com", Password="Anjali@123", Role="Employee", Status="Active", Phone="9876543206", Username="anjali01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Karan Joshi", Email="karan@gmail.com", Password="Karan@123", Role="Employee", Status="Active", Phone="9876543207", Username="karan01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Sneha Patel", Email="sneha@gmail.com", Password="Sneha@123", Role="Employee", Status="Active", Phone="9876543208", Username="sneha01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Vikas Kumar", Email="vikas@gmail.com", Password="Vikas@123", Role="Employee", Status="Active", Phone="9876543209", Username="vikas01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Pooja Yadav", Email="pooja@gmail.com", Password="Pooja@123", Role="Employee", Status="Active", Phone="9876543210", Username="pooja01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Deepak Saini", Email="deepak@gmail.com", Password="Deepak@123", Role="Employee", Status="Active", Phone="9876543211", Username="deepak01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Riya Jain", Email="riya@gmail.com", Password="Riya@123", Role="Employee", Status="Active", Phone="9876543212", Username="riya01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Arjun Kapoor", Email="arjun@gmail.com", Password="Arjun@123", Role="Employee", Status="Active", Phone="9876543213", Username="arjun01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Meera Nair", Email="meera@gmail.com", Password="Meera@123", Role="Employee", Status="Active", Phone="9876543214", Username="meera01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Nitin Chauhan", Email="nitin@gmail.com", Password="Nitin@123", Role="Employee", Status="Active", Phone="9876543215", Username="nitin01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Simran Kaur", Email="simran@gmail.com", Password="Simran@123", Role="Employee", Status="Active", Phone="9876543216", Username="simran01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Yash Agarwal", Email="yash@gmail.com", Password="Yash@123", Role="Employee", Status="Active", Phone="9876543217", Username="yash01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Kavita Mishra", Email="kavita@gmail.com", Password="Kavita@123", Role="Employee", Status="Active", Phone="9876543218", Username="kavita01", JoinDate=DateTime.UtcNow },
        new Employee { Name="Harsh Gupta", Email="harsh@gmail.com", Password="Harsh@123", Role="Employee", Status="Active", Phone="9876543219", Username="harsh01", JoinDate=DateTime.UtcNow }
    };

            _context.Employees.AddRange(employees);
            _context.SaveChanges();

            return Ok("20 employees added successfully.");
        }
    }
}


