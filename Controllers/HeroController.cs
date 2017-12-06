using System.Text;
using Microsoft.AspNetCore.Mvc;
using Heroes.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Heroes.Controllers
{
    [Route("api/[controller]")]
    public class HeroController : Controller
    {
        string imgStorage = "/images/heroes/";

        private readonly IConfiguration _configuration;
        public HeroController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult> getTopHeroes()
        {   
            string heroList = "";
            try
            {
                using(HeroDbContext db = new HeroDbContext())
                {
                    /*var hero = await (from h in db.Heroes 
                                join v in db.Votes on h.HeroID equals v.HeroID into result1
                                from r1 in result1.DefaultIfEmpty() group r1 by new {h.HeroID,h.HeroName,h.HeroIMG} into result2
                                select new { result2.Key.HeroID, result2.Key.HeroName, result2.Key.HeroIMG,
                                HeroVoted = result2.Count(c => c != null) })*/

                    var hero = await (from h in db.Heroes 
                                let vCount = (from v in db.Votes where h.HeroID == v.HeroID select v).Count()
                                let cCount = (from c in db.Comments where h.HeroID == c.HeroID select c).Count()
                                select new { h.HeroID, h.HeroName,
                                HeroIMG = imgStorage + (h.HeroIMG != null && h.HeroIMG != "" ? h.HeroIMG : "noimages.jpg;"),
                                HeroVoted = vCount, HeroCommented = cCount })
                                .OrderByDescending(h => h.HeroVoted).AsNoTracking().Take(5).ToListAsync();
                    heroList = JsonConvert.SerializeObject(hero);
                }
            }
            catch(Exception)
            {
                return BadRequest();
            }
            return Ok(heroList);
        }

        [HttpGet("{skipNum}")]
        public async Task<ActionResult> getMoreHero(int skipNum)
        {   
            string heroList = "";
            try
            {
                using(HeroDbContext db = new HeroDbContext())
                {

                    var hero = await (from h in db.Heroes 
                                let vCount = (from v in db.Votes where h.HeroID == v.HeroID select v).Count()
                                let cCount = (from c in db.Comments where h.HeroID == c.HeroID select c).Count()
                                select new { h.HeroID, h.HeroName,
                                HeroIMG = imgStorage + (h.HeroIMG != null && h.HeroIMG != "" ? h.HeroIMG : "noimages.jpg;"),
                                HeroVoted = vCount, HeroCommented = cCount })
                                .OrderByDescending(h => h.HeroVoted).AsNoTracking().Skip(skipNum).Take(4).ToListAsync();
                    heroList = JsonConvert.SerializeObject(hero);
                }
            }
            catch(Exception)
            {
                return BadRequest();
            }
            return Ok(heroList);
        }

        [HttpGet("detail/{heroid}")]
        public async Task<ActionResult> getHeroDetail(string heroid)
        {   
            string heroDetail = "";
            try
            {
                using(HeroDbContext db = new HeroDbContext())
                {
                    var hero = await (from h in db.Heroes where h.HeroID == heroid
                                join v in db.Votes on h.HeroID equals v.HeroID into result1
                                from r1 in result1.DefaultIfEmpty() group r1 by new {h.HeroID,h.HeroName,h.HeroDesc,h.HeroIMG} into result2
                                select new HeroDetail { HeroID = result2.Key.HeroID, HeroName = result2.Key.HeroName, HeroDesc = result2.Key.HeroDesc,
                                HeroIMG = (result2.Key.HeroIMG != null &&  result2.Key.HeroIMG != "" ? result2.Key.HeroIMG : "noimages.jpg;"),
                                HeroVoted = result2.Count(c => c != null) }).AsNoTracking().FirstOrDefaultAsync();
                    
                    if(hero != null)
                    {
                        string[] HeroIMG = hero.HeroIMG.Split(";");
                        HeroIMG = HeroIMG.Take(HeroIMG.Count()-1).ToArray(); //ไม่เอา array element ช่องสุดท้าย

                        hero.HeroIMG = "";
                        foreach(var h in HeroIMG)
                        {
                            hero.HeroIMG += imgStorage + h + ";";
                        }
                    }
                    


                    heroDetail = JsonConvert.SerializeObject(hero);
                }
            }
            catch(Exception)
            {
                return BadRequest();
            }
            return Ok(heroDetail);
        }

        [HttpGet("search/{heroid}")]
        public async Task<ActionResult> GetSearchHero(string heroid)
        {
            string heroDetail = "";
            try
            {
                using(HeroDbContext db = new HeroDbContext())
                {
                    var hero = await (from h in db.Heroes where h.HeroName.Contains(heroid) 
                                        select new { h.HeroID, h.HeroName}).AsNoTracking().ToListAsync();
                    
                    heroDetail = JsonConvert.SerializeObject(hero);
                }
            }
            catch(Exception)
            {
                return BadRequest();
            }
            return Ok(heroDetail);
        }

        [HttpGet("comment/{heroid}")]
        public async Task<ActionResult> GetHeroComment(string heroid)
        {
            string Comments = "";
            try
            {
                using(HeroDbContext db = new HeroDbContext())
                {
                    var hero = await (from c in db.Comments where c.HeroID == heroid
                                    select new { c.CommentID, CommentGroup = (c.ParentID != 0 ? c.ParentID : c.CommentID),
                                    //c.Name, c.Email, c.Message, CommentDate = c.CommentDate.ToString("d MMMM yyyy เวลา HH.mm น.")})
                                    c.Name, c.Email, c.Message, CommentDate = c.CommentDate})
                                    .OrderBy(c => c.CommentGroup).ThenBy(c => c.CommentID).AsNoTracking().ToListAsync();
                    
                    Comments = JsonConvert.SerializeObject(hero);
                }
            }
            catch(Exception)
            {
                return BadRequest();
            }
            return Ok(Comments);
        }

        [HttpPost]
        public async Task<ActionResult> InsertVote([FromBody]string heroid)
        {
            string IPAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            string heroDetail = "";
            try
            {
                using(HeroDbContext db = new HeroDbContext())
                {
                    SqlParameter[] parameters = new SqlParameter[] { 
                        new SqlParameter("@HeroID", SqlDbType.VarChar, 5) { Value=heroid },
                        new SqlParameter("@IPAddress", SqlDbType.VarChar, 30) { Value=IPAddress }
                    };
                    var hero = await db.Votes.FromSql("EXEC InsertVote @HeroID, @IPAddress", parameters)
                    .Select(h =>  h.HeroID).AsNoTracking().FirstOrDefaultAsync();
                    
                    heroDetail = JsonConvert.SerializeObject(hero);
                }
            }
            catch(Exception)
            {
                return BadRequest();
            }
            return Ok(heroDetail);
        }

        [HttpPost("comment")]
        public async Task<ActionResult> InsertComment([FromBody]PostComment comment)
        {
            bool status = false;
            if(ModelState.IsValid)
            {
                
                try
                {
                    using(HeroDbContext db = new HeroDbContext())
                    {
                        string IP = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                        DateTime date = DateTime.Now;
                        int heroCount = await db.Heroes.Where(h => h.HeroID == comment.HeroID).CountAsync();

                        if(heroCount > 0)
                        {
                            Comments cm = new Comments() {
                                HeroID = comment.HeroID,
                                Name = comment.Name.Trim(),
                                Email = comment.Email.Trim(),
                                Message = comment.Message.Trim(),
                                IPAddress = IP,
                                CommentDate = date
                            };

                            db.Comments.Add(cm);
                            int row = await db.SaveChangesAsync();

                            if(row > 0) status = true;
                        }
                    }
                }
                catch(Exception)
                {
                    return BadRequest();
                }
            }
            return Ok(status);
        }

        [HttpPost("login")]
        public IActionResult AdminLogin([FromBody]Account loginViewModel)
        {
            if (ModelState.IsValid)
            {
                Task<bool> userId = GetUserIdFromCredentials(loginViewModel);
                userId.Wait();
                if (userId.Result == false)
                {
                    return Ok(null);
                }

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, loginViewModel.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken
                (
                    issuer: _configuration["Issuer"],
                    audience: _configuration["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(60),
                    notBefore: DateTime.UtcNow,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SigningKey"])),
                         SecurityAlgorithms.HmacSha256)
                );

                return Ok(JsonConvert.SerializeObject(new JwtSecurityTokenHandler().WriteToken(token)));
            }

            return BadRequest();
        }
        [Authorize]
        [HttpDelete("comment")]
        public ActionResult DeleteComment([FromBody]Account commentid)
        {
            bool status = false;
                
            try
            {
                using(HeroDbContext db = new HeroDbContext())
                {
                    /*var comments = await (from c in db.Comments where c.CommentID == commentid select c).FirstOrDefaultAsync();
                    db.Comments.Remove(comments);
                    var row = await db.SaveChangesAsync();
                    if(row > 0) status = true;*/
                }
                
            }
            catch(Exception)
            {
                return BadRequest();
            }
            return Ok(status);
        }


        //Check Username Password for Login
        private async Task<bool> GetUserIdFromCredentials(Account loginViewModel)
        {
            bool userId = false;
            try
            {
                using(HeroDbContext db = new HeroDbContext())
                {
                    DateTime date = DateTime.Now;

                    var userdb = await (from u in db.Admins where u.Username == loginViewModel.Username select u).FirstOrDefaultAsync();
                    
                    if(userdb != null)
                    {
                        if((loginViewModel.Username == userdb.Username) && (loginViewModel.Password == userdb.Password))
                        {
                            userId = true;
                        }
                    }
                }
            }
            catch(Exception) { }

            return userId;
        }



        
    }
    
}