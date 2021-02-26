﻿using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }
        
        
        /// <summary>
        /// Get all authors
        /// </summary>
        /// <returns>Lista de authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempted Get all authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Succesfully got all authors");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
           
        }

        /// <summary>
        /// Get an author by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An author's record</returns>
        [HttpGet("{id}")]
          [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted to get author with id: {id}");
                var author = await _authorRepository.FindById(id);

                if (author == null)
                {
                    _logger.LogWarn($"Author with id: {id} was not found");
                    return NotFound();
                }

                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"Succesfully got author with id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");                
            }           

        }

        /// <summary>
        /// Creates an author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author submission attempted");

                if (authorDTO == null)
                {
                    _logger.LogWarn($"Empty Request was submitted");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Author data was incomplete");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);

                if (!isSuccess)
                {                    
                    return InternalError("Author creation failed");
                }

                _logger.LogInfo("Author created");
                return Created("Create", new { author });

            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

       /// <summary>
       /// Update an author
       /// </summary>
       /// <param name="id"></param>
       /// <param name="authorDTO"></param>
       /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author updated attempted - id: {id}");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn("Author update failed with bad data");
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Author data was incomplete");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);
                
                if (!isSuccess)
                {
                    return InternalError($"Update operation failed");
                }
                return NoContent();

            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        
        }


        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the admin");
        }
    }
}
