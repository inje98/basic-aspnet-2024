using basicDbHandling.Models;
using Microsoft.EntityFrameworkCore;

namespace basicDbHandling.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            // 내용무
        }

        // CodeFirst로 만들어둔 엔티티클래스를 작성, 테이블 생성할 엔티티가 늘때마다 여기에 추가해야함
        public DbSet<Category> Categories { get; set; }
    }
}
