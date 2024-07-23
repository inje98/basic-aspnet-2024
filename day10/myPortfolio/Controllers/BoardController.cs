using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using myPortfolio.Data;
using myPortfolio.Models;

namespace myPortfolio.Controllers
{
    public class BoardController : Controller
    {
        private readonly AppDbContext _context;

        public BoardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Board
        // FromSql()로 작업시는 비동기 async, await, Task<> 를 걷어내야 함
        public IActionResult Index(int page = 1, string search = "")
        {
            
            var totalCount = _context.Board.FromSqlRaw<Board>($"SELECT *FROM Board WHERE Title LIKE '%{search}%'").Count();
            var CountList = 10; // 페이지별 게시글 수
            var totalPage = totalCount / CountList; // 총 페이지 수

            if (totalCount % CountList > 0) totalPage++; // 12 % 10 = 2 > 0 한페이지가 더 필요
            if (totalPage < page) page = totalPage; // 현재 페이지번호가 전체 페이지보다 크면 축소시켜줌

            var countPage = 10; // 밑에 페이지번호 수
            var startPage = (page - 1) / countPage * countPage + 1; // 1~10페이지, 11~20페이지 식으로
            var endPage = startPage + countPage - 1; // 1페이지부터 시작하면 10페이지가 마지막
            if (totalPage < endPage) endPage = totalPage; // 2페이지까지밖에 없으면 endPage 10 -> 2로 변경

            var startCount = ((page - 1) * countPage) + 1; // 1, 11, 21, ... 순으로
            var endCount = startCount + (countPage - 1); // 10, 20, 30, ... 순으로

            // ViewData(Dict), ViewBag(Prop) 변수 (뷰cshtml에서 사용할 변수)
            ViewBag.StartPage = startPage;
            ViewBag.EndPage = endPage;
            ViewBag.Page = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.TotalCount = totalCount; // 전체 글 갯수
            ViewBag.Search = search; // 검색결과

            //var StartCount = new SqlParameter("StartCount", startCount);
            //var EndCount = new SqlParameter("EndCount", endCount);
            var list = _context.Board.FromSqlRaw($@"
                SELECT *
                  FROM (
                      SELECT ROW_NUMBER() OVER (ORDER BY Id DESC) AS rowNum
                          , Id
                          , Name
                          , UserId
                          , Title
                          , Contents
                          , Hit
                          , RegDate
                          , ModDate
                        FROM Board
                       WHERE Title LIKE '%{search}%'
                      ) AS base
                 WHERE base.rowNum BETWEEN {startCount} AND {endCount}
                    ").ToList();

            return View(list);
        }

        // GET: Board/Details/5
        // 게시글 상세 읽기
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var board = await _context.Board
                .FirstOrDefaultAsync(m => m.Id == id);
            if (board == null)
            {
                return NotFound();
            }
            // 게시글 조회수를 1 증가
            board.Hit += 1;
            _context.Update(board); // 객체에 내용 반영
            await _context.SaveChangesAsync(); // 실제 DB를 변경

            return View(board); // 게시글 하나를 뷰로 던져줘
        }

        // GET: Board/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Board/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,UserId,Title,Contents,Hit,RegDate,ModDate")] Board board)
        {
            // 아무값도 입력하지 않으면 ModelState.IsValid는 false
            if (ModelState.IsValid)
            {
                board.RegDate = DateTime.Now;
                //board.ModDate = null;
                _context.Add(board);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(board);
        }

        // GET: Board/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var board = await _context.Board.FindAsync(id); // SELECT * FROM WHERE
            if (board == null)
            {
                return NotFound();
            }
            return View(board);
        }

        // POST: Board/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,UserId,Title,Contents,Hit,RegDate,ModDate")] Board board)
        {
            if (id != board.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 수정날짜 추가!
                    board.ModDate = DateTime.Now;
                    _context.Update(board);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoardExists(board.Id))
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
            return View(board);
        }

        // GET: Board/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var board = await _context.Board
                .FirstOrDefaultAsync(m => m.Id == id);
            if (board == null)
            {
                return NotFound();
            }

            return View(board);
        }

        // POST: Board/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var board = await _context.Board.FindAsync(id);
            if (board != null)
            {
                _context.Board.Remove(board);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BoardExists(int id)
        {
            return _context.Board.Any(e => e.Id == id);
        }
    }
}
