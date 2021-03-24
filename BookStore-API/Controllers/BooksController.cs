using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Interacts with the books table
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all books
        /// </summary>
        /// <returns>A list of books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted call");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location}: Successful");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Gets a book by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A book record</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted call for {id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($"{location}: failed to retrieve record with id: {id}");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _logger.LogInfo($"{location}: Successful got record with id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Creates a new book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns>Book object</returns>
        [HttpPost]
        [Authorize(Roles ="Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Create attempted");

                if (bookDTO == null)
                {
                    _logger.LogWarn($"{location}: empty request was submitted");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: data was incomplete");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);

                if (!isSuccess)
                {
                    return InternalError($"{location}: creation failed");
                }
                _logger.LogInfo($"{location}: creation was successful");
                _logger.LogInfo($"{location}: {book}");
                return Created("Create", new { book });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Update attempted on record with id: {id}");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _logger.LogInfo($"{location}: update failed with bad data - id: {id}");
                    return BadRequest();
                }
                
                var isExists = await _bookRepository.IsExists(id);
                if (!isExists)
                {
                    return InternalError($"{location}: update failed");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: data was incomplete");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);
                if(!isSuccess)
                {
                    return InternalError($"{location}: update failed for record with id: {id}");
                }

                _logger.LogInfo($"{location}: record with id: {id} successfully updated");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Delete a book
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)        
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: delete attempted on record with id: {id}");

                if (id < 1)
                {
                    _logger.LogWarn($"{location} delete failed with bad data");
                    return BadRequest();
                }

                var isExists = await _bookRepository.IsExists(id);

                if (!isExists)
                {
                    _logger.LogWarn($"{location}: failed to retrieve record with id: {id}");
                    return NotFound();
                }

                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);

                if (!isSuccess)
                {
                    return InternalError($"{location}: delete failed for record with id: {id}");
                }
                _logger.LogInfo($"{location}: record with id: {id} successfully deleted");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the admin");
        }
    }
}
