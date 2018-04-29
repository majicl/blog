using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using crossblog.Domain;
using crossblog.Model;
using crossblog.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace crossblog.Controllers
{
    [Route("[controller]")]
    public class ArticlesController : Controller
    {
        private readonly IArticleRepository _articleRepository;

        public ArticlesController(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository;
        }

        // GET articles/search
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery]string title)
        {
            // this is a bad practice that we try to match a keyword within data with a cloumn about 32000 capacity
            // we must use indexed data to provide high-quality search, for example using Lucene as a full-featured text search engine library
            var articles = await _articleRepository
                .Query()
                .AsNoTracking()
                .Where(a => a.Title.Contains(title) || a.Content.Contains(title))
                .Take(20)
                .ToListAsync();

            var result = new ArticleListModel
            {
                Articles = articles.Select(a => new ArticleModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    Date = a.Date,
                    Published = a.Published
                })
            };

            return Ok(result);
        }

        // GET articles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var article = await _articleRepository.GetAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            var result = new ArticleModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Date = article.Date,
                Published = article.Published
            };

            return Ok(result);
        }

        // POST articles
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ArticleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = new Article
            {
                Title = model.Title,
                Content = model.Content,
                Date = model.Date,
                Published = model.Published
            };

            await _articleRepository.InsertAsync(article);

            return Created($"articles/{article.Id}", article);
        }

        // PUT articles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]ArticleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await _articleRepository.GetAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            article.Title = model.Title;
            article.Content = model.Content;
            article.Date = DateTime.UtcNow;
            article.Published = model.Published;

            await _articleRepository.UpdateAsync(article);

            return Ok(article);
        }

        // DELETE articles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _articleRepository.GetAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            await _articleRepository.DeleteAsync(id);

            return Ok();
        }
    }
}